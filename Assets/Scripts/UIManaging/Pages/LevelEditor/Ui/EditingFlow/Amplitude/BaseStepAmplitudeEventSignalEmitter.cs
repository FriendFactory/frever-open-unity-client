using Modules.Amplitude.Signals;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.DressUp;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.Signals;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.Amplitude
{
    internal abstract class BaseStepAmplitudeEventSignalEmitter<TButtonClickedSignal, TProgressTracker, TProgressStep, TProgressStepType> : BaseAmplitudeEventSignalEmitter
        where TButtonClickedSignal : BaseSelectionButtonClickedSignal<TProgressStepType>
        where TProgressTracker : BaseEditingStepProgressTracker<TProgressStep>
        where TProgressStep : IProgressStep
    {
        private readonly TProgressTracker _progressTracker;

        protected BaseStepAmplitudeEventSignalEmitter(SignalBus signalBus, TProgressTracker progressTracker) : base(signalBus)
        {
            _progressTracker = progressTracker;
        }

        public override void Initialize()
        {
            SignalBus.Subscribe<TButtonClickedSignal>(OnSelectionButtonClicked);

            _progressTracker.ProgressChanged += OnProgressChangedInternal;
        }

        public override void Dispose()
        {
            SignalBus.Unsubscribe<TButtonClickedSignal>(OnSelectionButtonClicked);
            
            _progressTracker.ProgressChanged -= OnProgressChangedInternal;
        }
        
        protected abstract void OnSelectionButtonClicked(TButtonClickedSignal signal);
        protected abstract void OnProgressChanged(IProgressStep step);

        private void OnProgressChangedInternal(IProgressStep step)
        {
            if (!step.IsCompleted) return;
            
            OnProgressChanged(step);
        }
    }
}