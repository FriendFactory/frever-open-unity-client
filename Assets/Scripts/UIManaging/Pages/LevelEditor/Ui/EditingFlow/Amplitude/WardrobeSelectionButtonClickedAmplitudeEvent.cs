using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.Amplitude
{
    internal sealed class WardrobeSelectionButtonClickedAmplitudeEvent: BaseSelectionButtonClickedAmplitudeEvent<WardrobeSelectionProgressStepType>
    {
        public WardrobeSelectionButtonClickedAmplitudeEvent(WardrobeSelectionProgressStepType stepType) : base(stepType)
        {
        }

        protected override string GetName(WardrobeSelectionProgressStepType stepType)
        {
            return stepType.GetWardrobeSelectionButtonClickedEventName();
        }
    }
}