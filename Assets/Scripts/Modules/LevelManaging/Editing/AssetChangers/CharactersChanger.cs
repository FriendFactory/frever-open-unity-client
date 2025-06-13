using System;
using System.Linq;
using System.Threading;
using Bridge.Models.ClientServer.Assets;
using Development;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging;
using Modules.AssetsManaging.LoadArgs;
using Modules.CameraSystem.CameraSystemCore;
using Modules.FreverUMA;
using Modules.LevelManaging.Assets;
using UnityEngine;
using CharacterController = Models.CharacterController;
using Event = Models.Event;

namespace Modules.LevelManaging.Editing.AssetChangers
{
    [UsedImplicitly]
    internal sealed class CharactersChanger : BaseChanger
    {
        private readonly IAssetManager _assetManager;
        private readonly AvatarHelper _avatarHelper;
        private readonly LayerManager _layerManager;
        private readonly ICameraSystem _cameraSystem;
        private Action<ICharacterAsset> _onSuccess;
        private CharacterFullInfo _characterData;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public CharactersChanger(IAssetManager assetManager, AvatarHelper avatarHelper, LayerManager layerManager, ICameraSystem cameraSystem)
        {
            _assetManager = assetManager;
            _avatarHelper = avatarHelper;
            _layerManager = layerManager;
            _cameraSystem = cameraSystem;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SpawnCharacter(CharacterFullInfo character, Event @event, Action<ICharacterAsset> onSuccess)
        {
            _onSuccess = onSuccess;
            _characterData = character;
            
            var cancellationSource = new CancellationTokenSource();
            CancellationSources.Add(character.Id, cancellationSource);
            var args = new CharacterLoadArgs
            {
                CancellationToken = cancellationSource.Token,
                DeactivateOnLoad = true
            };
            InvokeAssetStartedUpdating(DbModelType.Character, character.Id);
            _assetManager.Load(character, args, asset => SpawnCharacterLoaded(asset, @event), Debug.LogWarning);
        }

        public void DespawnCharacter(CharacterFullInfo character, Event @event, Action onSuccess)
        {
            var asset = _assetManager.GetActiveAssets<ICharacterAsset>().LastOrDefault(x => x.Id == character.Id);
            _assetManager.Unload(asset, AssetUnloaded);
            
            void AssetUnloaded()
            {
                var setLocation = _assetManager.GetActiveAssets<ISetLocationAsset>().FirstOrDefault();
                var controllerToRemove = @event.CharacterController.First(cc => cc.CharacterId == character.Id);
                @event.CharacterController.Remove(controllerToRemove);
                UpdateCharacterSequenceNumbers(@event, controllerToRemove);
                UpdateTargetCharacterSequenceNumber(@event, controllerToRemove);
                UpdateCharacterSpawnPositions(@event);
                UpdateCharacters(setLocation, @event);
                UpdateCharacterLayers();
                onSuccess?.Invoke();
            }
        }

        private void UpdateTargetCharacterSequenceNumber(Event ev, CharacterController removedController)
        {
            if (ev.GetCharactersCount() == 1)
            {
                ev.TargetCharacterSequenceNumber = ev.CharacterController.First().ControllerSequenceNumber;
                return;
            }

            if (ev.IsGroupFocus()) return;
            
            var wasRemovedCharacterFocused = ev.TargetCharacterSequenceNumber == removedController.ControllerSequenceNumber;
            if (!wasRemovedCharacterFocused) return;
            var maxSequenceNumber = ev.CharacterController.Max(x => x.ControllerSequenceNumber);
            ev.TargetCharacterSequenceNumber =
                Mathf.Clamp(ev.TargetCharacterSequenceNumber - 1, 0, maxSequenceNumber);
        }

        private void UpdateCharacterSpawnPositions(Event ev)
        {
            var spawnPos = ev.GetTargetSpawnPosition();
            if (!spawnPos.HasGroup() || !spawnPos.AllowUsingSubSpawnPositions) return;

            SetupCharactersOnSpawnPositionsFromGroup(ev, ev.GetOrderedCharacterControllers());
        }

        private static void SetupCharactersOnSpawnPositionsFromGroup(Event ev, CharacterController[] characterControllers)
        {
            var groupId = ev.GetTargetSpawnPosition().GetGroupId();
            var spawnPositionGroup = ev.GetSetLocation().GetSpawnPositionsGroup(groupId).ToArray();
            for (var i = 0; i < characterControllers.Length; i++)
            {
                characterControllers[i].CharacterSpawnPositionId = spawnPositionGroup.ElementAt(i).Id;
            }
        }

        private static void UpdateCharacterSequenceNumbers(Event @event, CharacterController controllerToRemove)
        {
            for (var i = controllerToRemove.ControllerSequenceNumber; i < @event.CharacterController.Count; i++)
            {
                @event.CharacterController.ElementAt(i).ControllerSequenceNumber--;
            }
        }

        public void ReplaceCharacter(CharacterFullInfo oldCharacter, CharacterFullInfo newCharacter, Event @event, bool unloadReplaced, Action<ICharacterAsset> onSuccess)
        {
            _onSuccess = onSuccess;
            _characterData = newCharacter;

            if (unloadReplaced)
            {
                _cameraSystem.ForgetLookAtTarget();
                _assetManager.Unload(oldCharacter, LoadNew);
            }
            else
            {
                LoadNew();
            }

            void LoadNew()
            {
                var characterController = @event.CharacterController.FirstOrDefault(controller => controller.Character.Id == oldCharacter.Id);
                InvokeAssetStartedUpdating(DbModelType.Character, newCharacter.Id);
                _assetManager.Load(newCharacter, asset => ReplaceCharacterLoaded(asset, @event, characterController), Debug.LogWarning);
            }
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void UpdateCharacters(ISetLocationAsset setLocation, Event ev)
        {
            var characterIds = ev.GetUniqueCharacterIds();
            var characters = _assetManager.GetAllLoadedAssets<ICharacterAsset>().Where(x=>characterIds.Contains(x.Id));
            foreach (var character in characters)
            {
                var characterController = ev.GetCharacterControllerByCharacterId(character.Id);
                var spawnPositId = characterController.CharacterSpawnPositionId;
                var spawnPositModel = setLocation.RepresentedModel.GetSpawnPosition(spawnPositId);
                setLocation.Attach(spawnPositModel, character);
                setLocation.ResetPosition(character);
            }

            DevUtils.ApplyShadersWorkaroundForWinEditor(setLocation.SceneName);
        }

        private void SpawnCharacterLoaded(IAsset asset, Event @event)
        {
            var characterAsset = asset as ICharacterAsset;
            UpdateCharacterSpawnPositions(@event);
            SetupNewSpawnedCharacter(characterAsset, @event);
            _avatarHelper.UnloadAllUmaBundles();
            _onSuccess?.Invoke(characterAsset);
            _onSuccess = null;
            InvokeAssetUpdated(DbModelType.Character);
            CancellationSources.Remove(asset.Id);
        }

        private void ReplaceCharacterLoaded(IAsset asset, Event ev, CharacterController characterController)
        {
            var characterAsset = asset as ICharacterAsset;
            SetupReplacedCharacter(characterAsset, ev, characterController);
            _avatarHelper.UnloadAllUmaBundles();
            _onSuccess?.Invoke(characterAsset);
            _onSuccess = null;
            InvokeAssetUpdated(DbModelType.Character);
        }

        private void SetupReplacedCharacter(ICharacterAsset asset, Event ev, CharacterController characterController)
        {
            characterController.Character = _characterData;
            characterController.CharacterId = _characterData.Id;
            SetupCharacterInternal(asset, ev, characterController.ControllerSequenceNumber);
        }
        
        private void SetupNewSpawnedCharacter(ICharacterAsset asset, Event @event)
        {
            var controller = GetControllerForNewCharacter(@event);
            @event.CharacterController.Add(controller);
            SetupCharacterInternal(asset, @event, controller.ControllerSequenceNumber);
        }
        
        private CharacterController GetControllerForNewCharacter(Event @event)
        {
            var controller = @event.CharacterController.OrderBy(ch => ch.ControllerSequenceNumber).FirstOrDefault()?.Clone();
            controller.ControllerSequenceNumber = @event.CharacterController.Count;
            controller.Id = 0;
            controller.Character = _characterData;
            controller.CharacterId = _characterData.Id;
            controller.SetOutfit(null);

            var targetSpawnPosModel = @event.GetSetLocation().GetSpawnPosition(@event.CharacterSpawnPositionId);
            if (!targetSpawnPosModel.HasGroup())
            {
                controller.CharacterSpawnPositionId = targetSpawnPosModel.Id;
                return controller;
            }

            var spawnPointGroup = @event.GetSetLocation().GetSpawnPositionsGroup(targetSpawnPosModel.GetGroupId());
            var alreadyUsedSpawnPoses = @event.CharacterController.Select(x => x.CharacterSpawnPositionId).ToArray();
            var freeSpawnPos = spawnPointGroup.First(x => !alreadyUsedSpawnPoses.Contains(x.Id));
            controller.CharacterSpawnPositionId = freeSpawnPos.Id;
            return controller;
        }

        private void SetupCharacterInternal(ICharacterAsset asset, Event ev, int sequenceNumber)
        {
            var setLocation = _assetManager.GetActiveAssets<ISetLocationAsset>().FirstOrDefault();

            var layer = _layerManager.GetCharacterLayer(sequenceNumber);
            asset.SetLayer(layer);
            
            UpdateCharacters(setLocation, ev);
        }
        
        private void UpdateCharacterLayers()
        {
            var characters = _assetManager.GetActiveAssets<ICharacterAsset>();
            
            for (var i = 0; i < characters.Length; i++)
            {
                var layer = _layerManager.GetCharacterLayer(i);
                characters[i].SetLayer(layer);
            }
        }
    }
}
