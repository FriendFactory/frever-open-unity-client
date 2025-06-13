using System.Collections.Generic;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.TemplateSetup;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.Recording
{
    [UsedImplicitly]
    internal sealed class RecordingProgressTracker: BaseAssetSelectionProgressTracker
    {
        public RecordingProgressTracker(LevelEditorPageModel pageModel, ILevelEditor levelEditor, List<SelectAssetProgressStep> steps) : base(pageModel, levelEditor, steps)
        {
        }

        protected override void OnAssetLoaded(IEntity entity)
        {
            switch (entity)
            {
                case CameraAnimationFullInfo _:
                    CompleteStep(AssetSelectionProgressStepType.CameraAnimation);
                    break;
                
            }
        }
    }
}