using Modules.Amplitude.Events.Core;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.Amplitude
{
    internal sealed class DressUpProgressChangedAmplitudeEvent: BaseAmplitudeEvent
    {
        public override string Name => "dress_up_status_changed";

        public DressUpProgressChangedAmplitudeEvent(SelectWardrobeProgressStep step)
        {
            _eventProperties.Add("Button Name", step.StepType.GetWardrobeSelectionButtonName());
            _eventProperties.Add("Status Approved", step.IsCompleted.ToString());
        }
    }
}