using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking
{
    [UsedImplicitly]
    internal abstract class EditingFlowStepProgress<TProgressStep> : IEditingFlowStepProgress<TProgressStep> where TProgressStep : IProgressStep
    {
        private readonly List<TProgressStep> _steps;
        
        public bool IsInitialized { get; private set; }
        public IReadOnlyList<TProgressStep> Steps => _steps;
        public int StepsCount { get; }
        public int CompletedStepsCount { get; private set; }
        public TProgressStep CurrentStep { get; private set; }
        
        public event Action ProgressChanged;
        
        protected EditingFlowStepProgress(List<TProgressStep> steps)
        {
            _steps = steps;
            StepsCount = _steps.Count;
        }

        public void Initialize()
        {
            _steps.ForEach(step => step.StateChanged += OnStepStateChanged);
            
            CompletedStepsCount = _steps.Count(step => step.IsCompleted);
            CurrentStep = _steps.FirstOrDefault(step => !step.IsCompleted) ?? _steps.Last();
            
            IsInitialized = true;
        }

        public void CleanUp()
        {
            _steps.ForEach(step => {
                step.Reset();
                step.StateChanged -= OnStepStateChanged;
            });
            
            IsInitialized = false;
        }

        private void OnStepStateChanged(IProgressStep progressStep)
        {
            CompletedStepsCount = _steps.Count(step => step.IsCompleted);
            CurrentStep = _steps.FirstOrDefault(step => !step.IsCompleted) ?? _steps.Last();
            
            ProgressChanged?.Invoke();
        }
    }
}