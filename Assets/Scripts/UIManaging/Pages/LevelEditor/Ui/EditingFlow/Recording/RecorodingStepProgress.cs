using System.Collections.Generic;
using JetBrains.Annotations;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.Recording
{
    [UsedImplicitly]
    internal sealed class RecordingStepProgress : EditingFlowStepProgress<SelectAssetProgressStep>
    {
        public RecordingStepProgress(List<SelectAssetProgressStep> steps) : base(steps)
        {
        }
    }
}