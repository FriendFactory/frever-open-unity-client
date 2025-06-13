using System;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.CameraSystem.CameraSystemCore;
using Modules.CharacterManagement;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.PhotoBooth.Profile;
using UnityEngine;
using PlayMode = Modules.LevelManaging.Editing.Players.EventPlaying.PlayMode;

namespace Modules.ProfilePhotoEditing
{
    [UsedImplicitly]
    internal sealed class ProfilePhotoEditor : IProfilePhotoEditor
    {
        private readonly ILevelManager _levelManager;
        private readonly IProfilePhotoEditorDefaults _editorDefaults;
        private readonly CharacterManager _characterManager;
        private readonly ICameraSystem _cameraSystem;
        private readonly ProfilePhotoBooth _photoBooth;
        private readonly VirtualCameraBasedController _cameraController;
        private readonly ProfilePhotoBoothPresetProvider _presetProvider;
        private readonly IDataFetcher _dataFetcher;

        public bool IsReady { get; private set; }
        public bool EnableCamera
        {
            set
            {
                var camera = _levelManager.GetActiveCamera();
                if (camera) camera.enabled = value;
            }
        }

        private ProfilePhotoBoothPreset _preset;

        //---------------------------------------------------------------------
        // Ctors 
        //---------------------------------------------------------------------

        internal ProfilePhotoEditor(ILevelManager levelManager, IProfilePhotoEditorDefaults editorDefaults,
                                    CharacterManager characterManager, ICameraSystem cameraSystem,
                                    ProfilePhotoBooth photoBooth, VirtualCameraBasedController cameraController,
                                    ProfilePhotoBoothPresetProvider presetProvider, IDataFetcher dataFetcher)
        {
            _levelManager = levelManager;
            _editorDefaults = editorDefaults;
            _characterManager = characterManager;
            _cameraSystem = cameraSystem;
            _photoBooth = photoBooth;
            _cameraController = cameraController;
            _presetProvider = presetProvider;
            _dataFetcher = dataFetcher;
        }

        //---------------------------------------------------------------------
        // Public 
        //---------------------------------------------------------------------

        public async Task InitializeAsync(ProfilePhotoType photoType)
        {
            if (_presetProvider.TryGetPreset(photoType, out var preset))
            {
                _preset = MonoBehaviour.Instantiate(preset);
            }

            _levelManager.AllowReducingCharactersQuality = DeviceInformationHelper.IsLowEndDevice();
            var characterId = _characterManager.SelectedCharacter.Id;
            var character = await _characterManager.GetCharacterAsync(characterId);
            if (character == null)
            {
                throw new NullReferenceException();
            }

            var template = _editorDefaults.GetTemplate();

            _levelManager.Initialize(_dataFetcher.MetadataStartPack);

            _levelManager.TemplateApplyingCompleted += OnTemplateApplyingCompleted;

            var level = _levelManager.CreateLevelInstance();
            _levelManager.CurrentLevel = level;
            _levelManager.CreateFreshEventBasedOnTemplate(template, new[] { character });
            
            while (!IsReady)
            {
                await Task.Delay(25);
            }
        }

        public Task<Texture2D> GetPhotoAsync()
        {
            var activeCamera = _levelManager.GetActiveCamera();
            return _photoBooth.GetPhotoAsync(activeCamera, _preset.resolution);
        }

        public void Cleanup()
        {
            _levelManager.TemplateApplyingCompleted -= OnTemplateApplyingCompleted;
            _levelManager.BodyAnimationChanged -= _cameraController.UpdateTargetTransform;
            _levelManager.SetLocationChangeFinished -= OnSetLocationChanged;
            
            _levelManager.CleanUp();
            _levelManager.ClearTempFiles();
            _levelManager.UnloadAllAssets();
        }

        private void OnSetLocationChanged(ISetLocationAsset asset)
        {
            _cameraController.UpdateTargetTransform();
        }

        //---------------------------------------------------------------------
        // Helpers 
        //---------------------------------------------------------------------

        private async void OnTemplateApplyingCompleted()
        {
            _levelManager.TemplateApplyingCompleted -= OnTemplateApplyingCompleted;

            var bodyAnimation = await _editorDefaults.GetBodyAnimationAsync();

            // event should be played before any animation switching
            _levelManager.UnloadNotUsedByTargetEventAssets();
            _levelManager.PlayEvent(PlayMode.StayOnFirstFrame);

            _levelManager.SetFaceTracking(false);
            _levelManager.ChangeBodyAnimation(bodyAnimation, OnCompleted);

            void OnCompleted()
            {
                SwitchCameraController();
                SetCameraTargets();
                _levelManager.BodyAnimationChanged += _cameraController.UpdateTargetTransform;
                _levelManager.SetLocationChangeFinished += OnSetLocationChanged;

                IsReady = true;
            }
        }

        private void SwitchCameraController()
        {
            _cameraSystem.Enable(false);
            _cameraSystem.SetFocusDistance(0.1f);

            var race = _dataFetcher.MetadataStartPack.GetRaceByGenderId(_characterManager.SelectedCharacter.GenderId);
            _preset.verticalFOV /= race.Id == 1 ? 1f : 2f;
            
            _cameraController.UpdateCameraSettings(_preset);
        }

        private void SetCameraTargets()
        {
            //TODO: replace with more convenient way to get active character
            var characterAsset = _levelManager.GetCurrentCharactersAssets().First();

            _cameraController.SetTargetParent(characterAsset.GameObject.transform);
            _cameraController.SetCameraTargets(characterAsset.HeadBoneGameObject.transform,
                                         characterAsset.HeadBoneGameObject.transform);
        }
    }
}