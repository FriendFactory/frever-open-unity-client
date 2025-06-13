using System.Collections.Generic;
using JetBrains.Annotations;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking
{
    [UsedImplicitly]
    internal sealed class DressUpStepProgress: EditingFlowStepProgress<SelectWardrobeProgressStep>
    {
        public DressUpStepProgress(List<SelectWardrobeProgressStep> steps) : base(steps)
        {
        }
    }
}