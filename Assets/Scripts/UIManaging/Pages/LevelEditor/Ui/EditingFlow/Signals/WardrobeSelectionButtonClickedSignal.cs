using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.Signals
{
    internal sealed class WardrobeSelectionButtonClickedSignal: BaseSelectionButtonClickedSignal<WardrobeSelectionProgressStepType>
    {
        public WardrobeSelectionButtonClickedSignal(WardrobeSelectionProgressStepType stepType) : base(stepType)
        {
        }
    }
}