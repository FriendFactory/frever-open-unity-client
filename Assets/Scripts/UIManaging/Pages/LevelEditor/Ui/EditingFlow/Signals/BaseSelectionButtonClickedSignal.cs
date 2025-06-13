namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.Signals
{
    internal abstract class BaseSelectionButtonClickedSignal<TProgressStepType>
    {
        public TProgressStepType StepType { get; }

        protected BaseSelectionButtonClickedSignal(TProgressStepType stepType)
        {
            StepType = stepType;
        }
    }
}