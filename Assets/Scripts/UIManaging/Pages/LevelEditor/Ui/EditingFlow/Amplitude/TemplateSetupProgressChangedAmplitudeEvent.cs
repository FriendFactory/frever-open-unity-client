using Modules.Amplitude.Events.Core;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.Amplitude
{
    internal sealed class TemplateSetupProgressChangedAmplitudeEvent: BaseAmplitudeEvent
    {
        public override string Name => "create_challenge_status_changed";
        
        public TemplateSetupProgressChangedAmplitudeEvent(SelectAssetProgressStep step)
        {
            _eventProperties.Add("Button Name", step.StepType.GetAssetSelectionButtonName());
            _eventProperties.Add("Status Approved", step.IsCompleted.ToString());
        }
    }
}