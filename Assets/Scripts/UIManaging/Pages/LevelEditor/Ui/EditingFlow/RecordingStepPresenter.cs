using Common.Permissions;
using Extensions;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.Permissions;
using UIManaging.Pages.UmaEditorPage.Ui;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow
{
    internal class RecordingStepPresenter : BaseEditingStepPresenter
    {
        private readonly UmaLevelEditorPanelModel _umaLevelEditorPanelModel;
        private readonly ILevelManager _levelManager;
        private readonly CameraPermissionHandler _cameraPermissionHandler;
        
        private RecordingStepView RecordingStepView => (RecordingStepView) View;

        public RecordingStepPresenter(UmaLevelEditorPanelModel umaLevelEditorPanelModel, ILevelManager levelManager, CameraPermissionHandler cameraPermissionHandler)
        {
            _umaLevelEditorPanelModel = umaLevelEditorPanelModel;
            _levelManager = levelManager;
            _cameraPermissionHandler = cameraPermissionHandler;
        }

        protected override void OnShown()
        {
            base.OnShown();
            
            _umaLevelEditorPanelModel.PanelOpened += OnWardrobePanelOpened;
            _umaLevelEditorPanelModel.PanelClosed += OnWardrobePanelClosed;

            if (_cameraPermissionHandler.IsPermissionGranted)
            {
                EnableFaceTrackingIfNeeded();
            }
            else if (!_cameraPermissionHandler.IsStatusDetermined)
            {
                _cameraPermissionHandler.RequestPermission(OnPermissionRequested);
            }
        }

        protected override void OnHidden()
        {
            base.OnHidden();
            
            _umaLevelEditorPanelModel.PanelOpened -= OnWardrobePanelOpened;
            _umaLevelEditorPanelModel.PanelClosed -= OnWardrobePanelClosed;
        }
        
        private void OnWardrobePanelOpened() => _levelManager.SetupCharactersForEditing();
        private void OnWardrobePanelClosed() => _levelManager.StopCharacterEditingMode();

        void OnPermissionRequested(PermissionRequestResult result)
        {
            if (result.IsError || result.IsSkipped) return;

            if (result.PermissionStatus.IsGranted())
            {
                EnableFaceTrackingIfNeeded();
            }
        }
        
        private void EnableFaceTrackingIfNeeded()
        {
            var isNewLevel = _levelManager.CurrentLevel.IsEmpty();
            var enableFaceTracking = _cameraPermissionHandler.IsPermissionGranted 
                                  && (isNewLevel || _levelManager.GetLastEvent().HasFaceAnimation());
            _levelManager.SetFaceTracking(enableFaceTracking);
            
            RecordingStepView.RefreshFaceTrackStatus();
        }
    }
}