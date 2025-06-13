using System.Collections.Generic;
using JetBrains.Annotations;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking
{
    [UsedImplicitly]
    internal sealed class TemplateSetupStepProgress: EditingFlowStepProgress<SelectAssetProgressStep>
    {
        public TemplateSetupStepProgress(List<SelectAssetProgressStep> steps) : base(steps)
        {
        }
    }
}