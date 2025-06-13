using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.Signals
{
    internal sealed class WardrobeSelectionButtonClickedSignalEmitter: BaseSelectionButtonClickedSignalEmitter<WardrobeSelectionProgressStepType, WardrobeSelectionButtonClickedSignal>
    {
        protected override WardrobeSelectionButtonClickedSignal GetSignal(WardrobeSelectionProgressStepType stepType)
        {
            return new WardrobeSelectionButtonClickedSignal(stepType);
        }
    }
}