using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking
{
    internal sealed class TemplateSetupProgressPanel: BaseEditingStepProgressPanel<SelectAssetProgressStep>
    {
        protected override string GetStepDescription()
        {
            return _localization.GetTemplateSetupStepDescription(ContextData.CurrentStep.StepType);
        }
    }
}