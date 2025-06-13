using UIManaging.Pages.LevelEditor.Ui.EditingFlow.Recording;
using UIManaging.Pages.LevelEditor.Ui.FaceTrackingToggle;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow
{
    internal sealed class RecordingStepView: BaseEditingStepView
    {
        [SerializeField] private RecordingStepProgressPanel _progressPanel;
        [SerializeField] private FaceTrackingToggleButton _faceTrackingToggle;
        
        public override LevelEditorState State => LevelEditorState.Default;

        protected override void OnShow()
        {
            base.OnShow();
            
            _progressPanel.Initialize();
        }

        protected override void OnHide()
        {
            base.OnHide();

            _progressPanel.CleanUp();
        }
        
        public void RefreshFaceTrackStatus()
        {
            _faceTrackingToggle.RefreshFaceTrackStatus();
        }
    }
}