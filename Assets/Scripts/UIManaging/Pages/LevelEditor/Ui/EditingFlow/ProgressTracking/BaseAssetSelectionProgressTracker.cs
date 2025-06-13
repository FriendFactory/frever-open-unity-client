using System.Collections.Generic;
using System.Linq;
using Bridge.Models.Common;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.DressUp;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.TemplateSetup
{
    internal abstract class BaseAssetSelectionProgressTracker : BaseEditingStepProgressTracker<SelectAssetProgressStep>
    {
        private readonly LevelEditorPageModel _pageModel;
        private readonly ILevelEditor _levelEditor;

        protected BaseAssetSelectionProgressTracker(LevelEditorPageModel pageModel, ILevelEditor levelEditor, List<SelectAssetProgressStep> steps) : base(steps)
        {
            _pageModel = pageModel;
            _levelEditor = levelEditor;
        }

        public override void Initialize()
        {
            base.Initialize();

            _levelEditor.AssetLoaded += OnAssetLoaded;
        }

        public override void CleanUp()
        {
            _levelEditor.AssetLoaded -= OnAssetLoaded;
            
            base.CleanUp();
        }

        protected void CompleteStep(AssetSelectionProgressStepType stepType)
        {
            if (_pageModel.EditorState == LevelEditorState.None) return;
            
            var step = _steps.FirstOrDefault(step => step.StepType == stepType);
            step?.Validate(stepType);
        }

        protected abstract void OnAssetLoaded(IEntity entity);
    }
}