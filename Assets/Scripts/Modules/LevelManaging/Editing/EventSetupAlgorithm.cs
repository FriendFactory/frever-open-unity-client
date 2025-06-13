using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Development;
using Extensions;
using JetBrains.Annotations;
using Models;
using Modules.AssetsManaging;
using Modules.CameraSystem.CameraSystemCore;
using Modules.FreverUMA;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Assets.AssetHelpers;
using Modules.LevelManaging.Editing.AssetChangers;
using Modules.LevelManaging.Editing.AssetChangers.SpawnFormations;
using Modules.LevelManaging.Editing.CameraManaging.CameraSettingsManaging;
using Modules.LevelManaging.Editing.LevelPreview;
using Debug = UnityEngine.Debug;
using Event = Models.Event;

namespace Modules.LevelManaging.Editing
{
    [UsedImplicitly]
    internal sealed class EventSetupAlgorithm
    {
        public event Action EventLoadingStarted;
        public event Action EventLoadingComplete;
        
        private readonly IAssetManager _assetManager;
        private readonly ICameraSystem _cameraSystem;
        private readonly IVoiceFilterController _voiceFilterController;
        private readonly AvatarHelper _avatarHelper;
        private readonly CameraFocusManager _cameraFocusManager;
        private readonly VfxBinder _vfxBinder;
        private readonly CameraAnchorManager _cameraAnchorManager;
        private readonly CameraSettingProvider _cameraSettingProvider;
        private readonly CharacterSpawnFormationChanger _characterSpawnFormationChanger;
        private readonly ISpawnFormationProvider _spawnFormationProvider;
        private readonly ILevelEditorEventAssetsLoader _eventAssetsLoader;

        private Action _onCompleted;
        private Event _currentEvent;
        private bool _refocusCamera;
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public EventSetupAlgorithm(IAssetManager assetManager, ICameraSystem cameraSystem,
            IVoiceFilterController voiceFilterController, AvatarHelper avatarHelper, CameraFocusManager cameraFocusManager, 
            VfxBinder vfxBinder, CameraAnchorManager cameraAnchorManager, CameraSettingProvider cameraSettingProvider, 
            CharacterSpawnFormationChanger characterSpawnFormationChanger, ISpawnFormationProvider spawnFormationProvider, 
            ILevelEditorEventAssetsLoader eventAssetsLoader)
        {
            _assetManager = assetManager;
            _cameraSystem = cameraSystem;
            _voiceFilterController = voiceFilterController;
            _avatarHelper = avatarHelper;
            _cameraFocusManager = cameraFocusManager;
            _cameraAnchorManager = cameraAnchorManager;
            _vfxBinder = vfxBinder;
            _cameraSettingProvider = cameraSettingProvider;
            _characterSpawnFormationChanger = characterSpawnFormationChanger;
            _spawnFormationProvider = spawnFormationProvider;
            _eventAssetsLoader = eventAssetsLoader;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetupEvent(Event eventData, Action onCompleted, bool refocusCamera)
        {
            EventLoadingStarted?.Invoke();

            _refocusCamera = refocusCamera;
            _currentEvent = eventData;
            _onCompleted = onCompleted;
            _eventAssetsLoader.LoadAssets(eventData, false, SetupEvent);
        }
        
        public void ResetOnCompletedCallback()
        {
            _onCompleted = null;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SetupEvent(List<IAsset> eventAssets)
        {
            var setLocation = SetupSetLocation(eventAssets);
            
            SetVoiceFilter();
            var characterAssets = eventAssets.Where(x => x.AssetType == DbModelType.Character).Cast<ICharacterAsset>().ToArray();
            SetupCharacters(_currentEvent, setLocation, characterAssets, _currentEvent.CharacterController);
            
            var targetCharacterController = _currentEvent.GetTargetCharacterController() ?? _currentEvent.GetFirstCharacterController();

            var characterAsset = characterAssets.First(asset => asset.Id == targetCharacterController.CharacterId);
            var spawnPosition = _currentEvent.CurrentCharacterSpawnPosition();
            SetVfxOnCorrectPlace(setLocation, characterAsset, spawnPosition);
            SetCaptionsOnCorrectPlace(setLocation, spawnPosition);

            SetupCamera(_currentEvent, setLocation);
            SetupDayNightControllers(setLocation);
            SetupCameraFilter(_currentEvent, eventAssets);
            
            _onCompleted?.Invoke();
            EventLoadingComplete?.Invoke();
            //_avatarHelper.UnloadAllUmaBundles();
        }

        private ISetLocationAsset SetupSetLocation(List<IAsset> eventAssets)
        {
            var setLocation = eventAssets.First(x => x.AssetType == DbModelType.SetLocation) as ISetLocationAsset;
            setLocation.SetActive(true);

            DevUtils.ApplyShadersWorkaroundForWinEditor(setLocation.SceneName);
            return setLocation;
        }

        private void SetupCharacters(Event ev, ISetLocationAsset setLocation, ICharacterAsset[] characterAssets, ICollection<CharacterController> controllers)
        {
            SetCharactersOnCorrectPlace(ev, setLocation, characterAssets, controllers);
            InitializeCharacters(characterAssets);
        }

        private static void InitializeCharacters(ICharacterAsset[] characterAssets)
        {
            foreach (var character in characterAssets)
            {
                character.ResetHairPosition();
            }
        }

        private void SetVoiceFilter()
        {
            var voiceFilter = _currentEvent.GetVoiceFilter();
            _voiceFilterController.ChangeSilently(voiceFilter, null, Debug.LogError);
        }

        private void SetCharactersOnCorrectPlace(Event ev, ISetLocationAsset setLocation, ICharacterAsset[] characters, ICollection<CharacterController> controllers)
        {
            if (_currentEvent.CharacterSpawnPositionFormationId == null)
            {
                var defaultId = _spawnFormationProvider.GetDefaultSpawnFormationId(ev);
                _currentEvent.CharacterSpawnPositionFormationId = defaultId;
            }
            
            DetachAssetsFromSetLocationsExcept(setLocation.Id, characters);
            SetupCharactersOnSpawnPositions(ev, setLocation, characters, controllers);
        }

        private void SetupCharactersOnSpawnPositions(Event ev, ISetLocationAsset setLocation, ICharacterAsset[] characters,
            ICollection<CharacterController> controllers)
        {
            if (ev.AreCharactersOnTheSameSpawnPosition())
            {
                var targetSpawnPos = _currentEvent.CurrentCharacterSpawnPosition();
                setLocation.Attach(targetSpawnPos, characters);

                var charactersIds = characters.Select(c => c.Id).ToArray();
                var characterControllers = _currentEvent.GetCharacterControllersByCharactersIds(charactersIds);
                _characterSpawnFormationChanger.Run(_currentEvent.CharacterSpawnPositionFormationId, characters,
                                                    characterControllers, setLocation, targetSpawnPos);
                return;
            }
            
            foreach (var controller in controllers)
            {
                var setLocationModel = setLocation.RepresentedModel;
                var spawnPos = setLocationModel.GetSpawnPosition(controller.CharacterSpawnPositionId);
                var targetCharacter = characters.First(x => x.Id == controller.CharacterId);
                setLocation.Attach(spawnPos, targetCharacter);
                setLocation.ResetPosition(targetCharacter);
            }
        }

        private void SetVfxOnCorrectPlace(ISetLocationAsset setLocation, ICharacterAsset character, CharacterSpawnPositionInfo spawnPosition)
        {
            var vfx = _assetManager.GetActiveAssets<IVfxAsset>()?.FirstOrDefault();
            if (vfx == null) return;

            DetachAssetsFromSetLocationsExcept(setLocation.Id, vfx);
            setLocation.Attach(spawnPosition, vfx);
            _vfxBinder.Setup(vfx, character);
        }

        private void SetCaptionsOnCorrectPlace(ISetLocationAsset setLocation, CharacterSpawnPositionInfo spawnPosition)
        {
            var captionAssets = _assetManager.GetActiveAssets<ICaptionAsset>()?.ToArray();
            if (captionAssets.IsNullOrEmpty()) return;

            DetachAssetsFromSetLocationsExcept(setLocation.Id, captionAssets);
            setLocation.Attach(spawnPosition, captionAssets);
        }

        private void SetupCamera(Event ev, ISetLocationAsset setLocation)
        {
            _cameraSystem.SetCameraComponents(setLocation.Camera, setLocation.CinemachineBrain);

            var spawnPositionId = _currentEvent.CharacterSpawnPositionId;
            var spawnPosition = setLocation.RepresentedModel.GetSpawnPositions().First(x=>x.Id == spawnPositionId);
            UpdateCameraSetting(spawnPosition);
            UpdateCameraHeadingBias(setLocation, spawnPosition);

            var cameraController = _currentEvent.GetCameraController();
            PrepareCameraWithCameraController(cameraController);
            
            _cameraAnchorManager.SetAnchor(setLocation, ev.CharacterSpawnPositionId);
            _cameraFocusManager.UpdateCameraFocus(ev, _refocusCamera);
            _cameraSystem.EnableCameraRendering(true);
        }

        private void UpdateCameraSetting(CharacterSpawnPositionInfo spawnPosition)
        {
            var spaceSizeId = spawnPosition.SpawnPositionSpaceSizeId;
            var cameraSetting = _cameraSettingProvider.GetSettingWithSpawnPositionId(spaceSizeId);
            _cameraSystem.ChangeCameraSetting(cameraSetting);
        }
        
        private void UpdateCameraHeadingBias(ISetLocationAsset setLocation, CharacterSpawnPositionInfo spawnPosition)
        {
            var spawnPositionTransform = setLocation.GetCharacterSpawnPositionTransform(spawnPosition.UnityGuid);
            var cameraHeadingBias = spawnPositionTransform.transform.eulerAngles.y;
            _cameraSystem.SetHeadingBias(cameraHeadingBias);
        }

        private void PrepareCameraWithCameraController(CameraController cameraController)
        {
            if (cameraController == null) return;
            
            _cameraSystem.SetPlaybackSpeed(cameraController.TemplateSpeed.ToKilo());
            _cameraSystem.SetFocusDistance(cameraController.StartFocusDistance.ToKilo());
            _cameraSystem.SetNoiseProfile(cameraController.CameraNoiseSettingsIndex);
            _cameraSystem.SetFollow(cameraController.FollowAll);
            _cameraSystem.SetLookAt(cameraController.LookAtIndex == 1);
        }
        
        private void SetupDayNightControllers(ISetLocationAsset setLocation)
        {
            if(!setLocation.IsDayTimeControlSupported) return;
            
            var setLocationController = _currentEvent.GetSetLocationController();

            var timeOfDay = setLocationController.TimeOfDay;
            var timelapseSpeed = setLocationController.TimelapseSpeed;

            var dayNightController = setLocation.DayNightController;
            
            if (timeOfDay != null)
            {
                dayNightController.SetTime(timeOfDay.ToKilo());
            }

            if (timelapseSpeed != null)
            {
                dayNightController.SetSpeed(timelapseSpeed.ToKilo());
            }
        }
        
        private void SetupCameraFilter(Event currentEvent, ICollection<IAsset> eventAssets)
        {
            var cameraFilterController = currentEvent.GetCameraFilterController();
            if(cameraFilterController == null) return;

            var cameraFilterAsset = eventAssets.First(x => x.AssetType == DbModelType.CameraFilterVariant &&
                                                           x.Id == cameraFilterController.CameraFilterVariantId) as ICameraFilterVariantAsset;
            var value = cameraFilterController.CameraFilterValue/100f ?? 1;
            cameraFilterAsset.SetIntensity(value);
        }

        private void DetachAssetsFromSetLocationsExcept(long setLocationExceptionId, params IAttachableAsset[] assets)
        {
            var loadedSetLocations = _assetManager.GetAllLoadedAssets<ISetLocationAsset>();
            foreach (var setLocation in loadedSetLocations)
            {
                if (setLocation.Id == setLocationExceptionId) continue;

                DetachAssetsFromSetLocation(setLocation, assets);
            }
        }

        private void DetachAssetsFromSetLocation(ISetLocationAsset setLocation, params IAttachableAsset[] assets)
        {
            foreach (var asset in assets)
            {
                if (!setLocation.IsAttached(asset)) continue;
                    
                setLocation.Detach(asset);
            }
        }
    }
}