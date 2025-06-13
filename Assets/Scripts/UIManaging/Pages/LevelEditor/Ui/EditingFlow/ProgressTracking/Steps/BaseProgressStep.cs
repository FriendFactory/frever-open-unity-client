using System;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps
{
    internal abstract class BaseProgressStep<TSelectionStepType>: IProgressStep where TSelectionStepType: Enum 
    {
        private bool _isCompleted;

        public virtual bool IsCompleted
        {
            get => _isCompleted;
            protected set
            {
                if (_isCompleted == value) return;
                
                _isCompleted = value;
                
                StateChanged?.Invoke(this);
            }
        }

        public TSelectionStepType StepType { get; }
        
        public virtual event Action<IProgressStep> StateChanged;

        protected BaseProgressStep(TSelectionStepType stepType)
        {
            StepType = stepType;
        }
        
        public virtual void Reset()
        {
            IsCompleted = false;
        }

        public abstract void Validate(TSelectionStepType selectionStepType);
    }
}