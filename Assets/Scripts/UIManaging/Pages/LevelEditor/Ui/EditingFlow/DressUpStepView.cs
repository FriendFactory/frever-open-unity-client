using JetBrains.Annotations;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.DressUp;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow
{
    internal sealed class DressUpStepView: BaseEditingStepView
    {
        [SerializeField] private DressUpPointingArrowController _pointingArrowController;
        [SerializeField] private DressUpProgressPanel _progressPanel;

        // TODO: move to a separate component, for example, ProgressPanel
        private DressUpStepProgress _progress;
        private DressUpStepProgressTracker _progressTracker;
        
        [Inject, UsedImplicitly]
        private void Construct(DressUpStepProgress progress, DressUpStepProgressTracker progressTracker)
        {
            _progress = progress;
            _progressTracker = progressTracker;
        }
        
        public override LevelEditorState State => LevelEditorState.Dressing;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            _progress.Initialize();
            _pointingArrowController.Initialize();
            _progressPanel.Initialize(_progress);
        }

        protected override void BeforeCleanUp()
        {
            base.BeforeCleanUp();
            
            _progress.CleanUp();
            _pointingArrowController.CleanUp();
            _progressPanel.CleanUp();
        }

        protected override void OnShow()
        {
            base.OnShow();
            
            _progressTracker.Initialize();
            
            _pointingArrowController.MoveToNextAvailablePosition();
        }
        
        protected override void OnHide()
        {
            base.OnHide();
            
            _progressTracker.CleanUp();
        }
        
    }
}