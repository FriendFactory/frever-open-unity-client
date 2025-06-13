using System;
using System.Collections.Generic;
using Abstract;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.DressUp
{
    internal abstract class BaseEditingStepProgressTracker<TProgressStep> : IInitializable where TProgressStep : IProgressStep
    {
        protected List<TProgressStep> _steps;
        
        public IReadOnlyList<TProgressStep> Steps => _steps;
        public bool IsInitialized { get; private set; }
        
        public virtual event Action<IProgressStep> ProgressChanged;
        
        protected BaseEditingStepProgressTracker(List<TProgressStep> steps)
        {
            _steps = steps;
        }

        public virtual void Initialize()
        {
            _steps.ForEach(step => step.StateChanged += OnStepStateChanged);
            
            IsInitialized = true;
        }

        public virtual void CleanUp()
        {
            _steps.ForEach(step => step.StateChanged -= OnStepStateChanged);
            
            IsInitialized = false;
        }

        protected virtual void OnStepStateChanged(IProgressStep progressStep) => ProgressChanged?.Invoke(progressStep);
    }
}