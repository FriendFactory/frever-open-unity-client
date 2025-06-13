using System;
using Bridge.Models.Common;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps
{
    internal interface IProgressStep
    {
        bool IsCompleted { get; }
        
        event Action<IProgressStep> StateChanged;

        void Reset();
    }
}