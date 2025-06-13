using System;
using JetBrains.Annotations;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.PhotoBooth.Character;
using UIManaging.Pages.UmaEditorPage.Ui;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow
{
    [UsedImplicitly]
    internal sealed class EditingFlowCameraSystemController: IInitializable, IDisposable
    {
        private readonly ICameraSystem _cameraSystem;
        private readonly UmaLevelEditorPanelModel _umaLevelEditorPanelModel;
        private readonly ILevelManager _levelManager;
        private readonly CharacterPhotoBooth _photoBooth;
        
        private int _cameraNoiseProfileId;

        public EditingFlowCameraSystemController(ICameraSystem cameraSystem, UmaLevelEditorPanelModel umaLevelEditorPanelModel,
            ILevelManager levelManager, CharacterPhotoBooth photoBooth)
        {
            _cameraSystem = cameraSystem;
            _umaLevelEditorPanelModel = umaLevelEditorPanelModel;
            _levelManager = levelManager;
            _photoBooth = photoBooth;
        }

        public void Initialize()
        {
            _umaLevelEditorPanelModel.PanelOpened += OnWardrobePanelOpened;
            _umaLevelEditorPanelModel.PanelClosed += OnWardrobePanelClosed;
        }

        public void Dispose()
        {
            _umaLevelEditorPanelModel.PanelOpened -= OnWardrobePanelOpened;
            _umaLevelEditorPanelModel.PanelClosed -= OnWardrobePanelClosed;
        }

        private void OnWardrobePanelOpened()
        {
            _cameraNoiseProfileId = _cameraSystem.GetNoiseProfileId();
            _cameraSystem.SetNoiseProfile(0);
            
            UpdateCameraPosition();
        }

        private void OnWardrobePanelClosed()
        {
            _cameraSystem.SetNoiseProfile(_cameraNoiseProfileId);
            
            // not sure if we need to reset PhotoBooth camera position to initial values
        }
        
        private void UpdateCameraPosition()
        {
            var character = _levelManager.EditingTargetCharacterAsset.GameObject.transform;

            var offset = new Vector3(0f, 1f, 3f);
            offset.Scale(character.forward);
            
            _photoBooth.transform.position = character.position;
            _photoBooth.transform.forward = -character.forward;
        }
    }
}