using System;
using System.Collections.Generic;
using Abstract;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking
{
    /// <summary>
    /// Represents a progress of editing flow step.
    /// </summary>
    internal interface IEditingFlowStepProgress<TProgressStep>: IInitializable where TProgressStep : IProgressStep
    {
        IReadOnlyList<TProgressStep> Steps { get; }

        bool IsCompleted => CompletedStepsCount == StepsCount;
        int StepsCount { get; }
        int CompletedStepsCount { get; }
        public TProgressStep CurrentStep { get; }
        
        event Action ProgressChanged;
    }
}