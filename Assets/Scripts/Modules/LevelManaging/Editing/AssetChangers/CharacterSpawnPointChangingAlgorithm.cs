using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.AssetChangers.SpawnFormations;
using CharacterController = Models.CharacterController;
using Event = Models.Event;
using TaskExtensions = Extensions.TaskExtensions;

namespace Modules.LevelManaging.Editing.AssetChangers
{
    [UsedImplicitly]
    internal sealed class CharacterSpawnPointChangingAlgorithm : BaseChanger
    {
        private readonly ICameraSystem _cameraSystem;
        private readonly EventAssetsProvider _eventAssetsProvider;
        private readonly CharacterSpawnFormationChanger _characterSpawnFormationChanger;
        private readonly BodyAnimationForSpawnPositionLoader _bodyAnimationForSpawnPositionLoader;
        private readonly BodyAnimationChanger _bodyAnimationChanger;
        private readonly BodyAnimationModelsLoader _bodyAnimationModelsLoader;
        private readonly IAssetManager _assetManager;
        private readonly SpawnFormationProvider _spawnFormationProvider;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private IBodyAnimationSelector BodyAnimationSelector { get; set; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public CharacterSpawnPointChangingAlgorithm(ICameraSystem cameraSystem, EventAssetsProvider eventAssetsProvider, CharacterSpawnFormationChanger characterSpawnFormationChanger, 
            BodyAnimationForSpawnPositionLoader bodyAnimationForSpawnPositionLoader, BodyAnimationChanger bodyAnimationChanger, IAssetManager assetManager,
            SpawnFormationProvider spawnFormationProvider, BodyAnimationModelsLoader bodyAnimationModelsLoader)
        {
            _cameraSystem = cameraSystem;
            _eventAssetsProvider = eventAssetsProvider;
            _characterSpawnFormationChanger = characterSpawnFormationChanger;
            _bodyAnimationForSpawnPositionLoader = bodyAnimationForSpawnPositionLoader;
            _bodyAnimationChanger = bodyAnimationChanger;
            _assetManager = assetManager;
            _spawnFormationProvider = spawnFormationProvider;
            _bodyAnimationModelsLoader = bodyAnimationModelsLoader;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetBodyAnimationSelector(IBodyAnimationSelector selector)
        {
            BodyAnimationSelector = selector;
        }
        
        public async Task RunAsync(CharacterSpawnPositionInfo nextMainSpawnPosition, Event ev, bool applyAnimations, bool keepFormation, Action<DbModelType[]> callback = null)
        {
            if (!GetSetLocationAsset(ev, out var setLocation)) return;
            
            CancelAll();
            var cancellationTokenSource = new CancellationTokenSource();
            CancellationSources[nextMainSpawnPosition.Id] = cancellationTokenSource;
            var token = cancellationTokenSource.Token;
            
            InvokeAssetStartedUpdating(DbModelType.CharacterSpawnPosition, nextMainSpawnPosition.Id);
            ev.CharacterSpawnPositionId = nextMainSpawnPosition.Id;

            if (applyAnimations)
            {
                await LoadAnimations(ev, token);
                if (token.IsCancellationRequested) return;
            }

            UpdateSpawnPositions(setLocation, nextMainSpawnPosition, ev, keepFormation);
            if (cancellationTokenSource.IsCancellationRequested) return;
            callback?.Invoke(applyAnimations && _bodyAnimationForSpawnPositionLoader.HasLoadedAnimations? new [] { DbModelType.BodyAnimation}: Array.Empty<DbModelType>());
            InvokeAssetUpdated(DbModelType.CharacterSpawnPosition);
        }

        private async Task LoadAnimations(Event ev, CancellationToken token)
        {
            var applyData = BodyAnimationSelector.Select();
            if (!applyData.CharacterToBodyAnimation.Any()) return;
           
            var animations = await _bodyAnimationModelsLoader.Load(applyData.CharacterToBodyAnimation.Values.ToArray(), token);
            _bodyAnimationForSpawnPositionLoader.LoadAnimations(animations, token);

            await WaitWhileLoadingAnimations(token);
            
            if (token.IsCancellationRequested) return;

            var characterIds = applyData.CharacterToSpawnPosition.Keys.ToArray();
            var targetControllers = ev.CharacterController.Where(x => characterIds.Contains(x.CharacterId));

            var args = new List<BodyAnimLoadArgs>();
            foreach (var controller in targetControllers)
            {
                var bodyAnimId = applyData.CharacterToBodyAnimation[controller.CharacterId];
                var bodyAnimation = _bodyAnimationForSpawnPositionLoader.LoadedAnimationModels.First(x => x.Id == bodyAnimId);
                var alreadyAddedArgs = args.FirstOrDefault(x => x.BodyAnimation.Id == bodyAnimation.Id);
                if (alreadyAddedArgs != null)
                {
                    alreadyAddedArgs.CharacterController.Add(controller);
                }
                else
                {
                    args.Add(new BodyAnimLoadArgs()
                    {
                        BodyAnimation = bodyAnimation,
                        CharacterController = new List<CharacterController> {controller}
                    });
                }
            }
            
            await _bodyAnimationChanger.Run(ev, args);
        }

        private async Task WaitWhileLoadingAnimations(CancellationToken token)
        {
            while (_bodyAnimationForSpawnPositionLoader.IsLoading && !token.IsCancellationRequested)
            {
                await TaskExtensions.DelayWithoutThrowingCancellingException(100, token);
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void UpdateSpawnPositions(ISetLocationAsset setLocation, CharacterSpawnPositionInfo spawnPosition, Event ev, bool keepFormation)
        {
            List<SpawnPositionAndCharacters> spawnPositionAndCharacter;
            
            var allCharacters = _eventAssetsProvider.GetLoadedAssets(ev, DbModelType.Character).Cast<ICharacterAsset>().ToArray();
            
            var putAllCharactersOnTheSamePoint = !spawnPosition.AllowUsingSubSpawnPositions || keepFormation || !spawnPosition.HasGroup() || ev.HasSetupMultiCharacterAnimation();
            if (putAllCharactersOnTheSamePoint)
            {
                spawnPositionAndCharacter = AttachAllCharactersToTheSameSpawnPosition(spawnPosition, ev);
            }
            else
            {
                spawnPositionAndCharacter = AttachCharactersToPositionsInGroup(setLocation, spawnPosition.GetGroupId(), ev);
            }

            foreach (var positionAndCharacters in spawnPositionAndCharacter)
            {
                var chars = allCharacters.Where(x => positionAndCharacters.Characters.Contains(x.Id)).ToArray();
                setLocation.Attach(positionAndCharacters.SpawnPosition, chars);
                setLocation.ResetPosition(chars);
            }
            
            foreach (var controller in ev.CharacterController)
            {
                var newPositionId = spawnPositionAndCharacter.First(x => x.Characters.Contains(controller.CharacterId)).SpawnPosition;
                controller.CharacterSpawnPositionId = newPositionId;
            }
            ev.SetCharacterSpawnPosition(spawnPosition);
            
            AttachVfxToNewPosition(setLocation, spawnPosition, ev);
            AttachCaptionsToNewPosition(setLocation, spawnPosition, ev);
            
            var spawnPositionTransform = setLocation.GetCharacterSpawnPositionTransform(spawnPosition.UnityGuid);
            var cameraHeadingBias = spawnPositionTransform.eulerAngles.y;
            _cameraSystem.UpdateFollowingState();
            _cameraSystem.SetHeadingBias(cameraHeadingBias);
            _cameraSystem.SetCameraAnchor(spawnPositionTransform);
            _cameraSystem.RecenterFocus();

            if (!putAllCharactersOnTheSamePoint) return;
           
            var nextSpawnFormationId = GetSpawnFormationForEvent(ev);
            _characterSpawnFormationChanger.Run(nextSpawnFormationId, ev);
        }

        private long GetSpawnFormationForEvent(Event ev)
        {
            var currentFormationId = ev.CharacterSpawnPositionFormationId;
            if (currentFormationId.HasValue)
            {
                var currentSpawnFormation = _spawnFormationProvider.GetSpawnPositionFormation(currentFormationId.Value);
                if (currentSpawnFormation.CharacterCount == ev.GetCharactersCount())
                {
                    return currentSpawnFormation.Id;
                }
            }

            return _spawnFormationProvider.GetDefaultSpawnFormationId(ev);
        }

        private static List<SpawnPositionAndCharacters> AttachAllCharactersToTheSameSpawnPosition(CharacterSpawnPositionInfo spawnPosition, Event ev)
        {
            return new List<SpawnPositionAndCharacters>
            {
                new SpawnPositionAndCharacters
                {
                    SpawnPosition = spawnPosition.Id, Characters = ev.GetUniqueCharacterIds()
                }
            };
        }

        private static List<SpawnPositionAndCharacters> AttachCharactersToPositionsInGroup(ISetLocationAsset setLocation, int groupId, Event ev)
        {
            var output = new List<SpawnPositionAndCharacters>();
            
            var setLocationModel = setLocation.RepresentedModel;
            var spawnPositionGroup = setLocationModel.GetSpawnPositionsGroup(groupId).ToArray();
            var controllers = ev.GetOrderedCharacterControllers();
            for (var index = 0; index < controllers.Length; index++)
            {
                var characterController = controllers[index];
                var characterId = characterController.CharacterId;
                var spawnPos = spawnPositionGroup[index];
                
                output.Add(new SpawnPositionAndCharacters
                {
                    SpawnPosition = spawnPos.Id,
                    Characters = new [] { characterId }
                });
            }

            return output;
        }

        private void AttachVfxToNewPosition(ISetLocationAsset setLocation, CharacterSpawnPositionInfo spawnPosition, Event ev)
        {
            var vfxAssets = _eventAssetsProvider.GetLoadedAssets(ev, DbModelType.Vfx).Cast<IVfxAsset>().ToArray();
            if (!vfxAssets.Any()) return;

            var vfx = vfxAssets.FirstOrDefault();
            DetachVfxFromOtherSetLocations(vfx, ev);
            setLocation.Attach(spawnPosition, vfx);
        }

        private void AttachCaptionsToNewPosition(ISetLocationAsset setLocation,
            CharacterSpawnPositionInfo spawnPosition, Event ev)
        {
            var captionAssets = _eventAssetsProvider.GetLoadedAssets(ev, DbModelType.Caption).Cast<ICaptionAsset>().ToArray();
            if (!captionAssets.Any()) return;

            var caption = captionAssets.FirstOrDefault();
            DetachCaptionsFromOtherSetLocations(caption, ev);
            setLocation.Attach(spawnPosition, caption);
        }

        private bool GetSetLocationAsset(Event ev, out ISetLocationAsset setLocation)
        {
            try
            {
                setLocation = _eventAssetsProvider.GetLoadedAssets(ev, DbModelType.SetLocation).First() as ISetLocationAsset;
            }
            catch (Exception e)
            {
                setLocation = null;
                UnityEngine.Debug.LogWarning(e);
                return false;
            }

            return true;
        }

        private void DetachVfxFromOtherSetLocations(IVfxAsset vfxAsset, Event currentEvent)
        {
            var otherLoadedSetLocations = GetOtherThanTargetLoadedSetLocations(currentEvent);
            foreach (var setLocation in otherLoadedSetLocations)
            {
                if (setLocation.IsAttached(vfxAsset))
                {
                    setLocation.Detach(vfxAsset);
                }
            }
        }
        
        private void DetachCaptionsFromOtherSetLocations(ICaptionAsset captionAsset, Event currentEvent)
        {
            var otherLoadedSetLocations = GetOtherThanTargetLoadedSetLocations(currentEvent);
            foreach (var setLocation in otherLoadedSetLocations)
            {
                if (setLocation.IsAttached(captionAsset))
                {
                    setLocation.Detach(captionAsset);
                }
            }
        }

        private IEnumerable<ISetLocationAsset> GetOtherThanTargetLoadedSetLocations(Event currentEvent)
        {
            var currentSetLocation = currentEvent.GetSetLocationId();
            var otherLoadedSetLocations = _assetManager.GetAllLoadedAssets<ISetLocationAsset>()
                                                       .Where(x => x.Id != currentSetLocation);
            return otherLoadedSetLocations;
        }
    }

    internal struct SpawnPositionAndCharacters
    {
        public long SpawnPosition;
        public long[] Characters;
    }
}