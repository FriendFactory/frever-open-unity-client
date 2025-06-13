using JetBrains.Annotations;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.DressUp;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.Signals;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.Amplitude
{
    [UsedImplicitly]
    internal sealed class DressUpStepAmplitudeEventSignalEmitter : BaseStepAmplitudeEventSignalEmitter<WardrobeSelectionButtonClickedSignal, DressUpStepProgressTracker, SelectWardrobeProgressStep, WardrobeSelectionProgressStepType>
    {
        public DressUpStepAmplitudeEventSignalEmitter(SignalBus signalBus, DressUpStepProgressTracker progressTracker) : base(signalBus, progressTracker)
        {
        }

        protected override void OnSelectionButtonClicked(WardrobeSelectionButtonClickedSignal signal)
        {
            Emit(new WardrobeSelectionButtonClickedAmplitudeEvent(signal.StepType));
        }

        protected override void OnProgressChanged(IProgressStep step)
        {
            if (step is not SelectWardrobeProgressStep selectWardrobeProgressStep) return;
            
            Emit(new DressUpProgressChangedAmplitudeEvent(selectWardrobeProgressStep));
        }
    }
}