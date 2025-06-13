using JetBrains.Annotations;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.Signals;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.TemplateSetup;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.Amplitude
{
    [UsedImplicitly]
    internal sealed class TemplateSetupStepAmplitudeEventSignalEmitter: BaseStepAmplitudeEventSignalEmitter<AssetSelectionButtonClickedSignal, TemplateSetupProgressTracker, SelectAssetProgressStep, AssetSelectionProgressStepType>
    {
        public TemplateSetupStepAmplitudeEventSignalEmitter(SignalBus signalBus, TemplateSetupProgressTracker progressTracker) : base(signalBus, progressTracker) { }

        // TODO: move asset selection button clicked signal namespace to a dedicated emitter, because the same signal is used on Recording step
        protected override void OnSelectionButtonClicked(AssetSelectionButtonClickedSignal signal)
        {
            // uses only for Character button - all other asset buttons use old amplitude events
            Emit(new AssetSelectionButtonClickedAmplitudeEvent(signal.StepType));
        }

        protected override void OnProgressChanged(IProgressStep step)
        {
            if (step is not SelectAssetProgressStep selectAssetProgressStep) return;
            
            Emit(new TemplateSetupProgressChangedAmplitudeEvent(selectAssetProgressStep));
        }
    }
}