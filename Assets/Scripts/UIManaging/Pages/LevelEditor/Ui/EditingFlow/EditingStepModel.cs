using System;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow
{
    internal class EditingStepModel
    {
        public LevelEditorState State { get; }
        
        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (_isCompleted == value) return;
                
                _isCompleted = value;
                
                CompletedStateChanged?.Invoke(_isCompleted);
            }
        }

        public bool IsFirstInFlow
        {
            get => _isFirstInFlow;
            set
            {
                if (_isFirstInFlow == value) return;
                
                _isFirstInFlow = value;
                
                FirstInFlowChanged?.Invoke(value);
            }
        }
        
        public bool IsExiting { get; set; }

        private bool _isCompleted;
        private bool _isFirstInFlow;

        public event Action MoveBack;
        public event Action MoveNext;
        public event Action MoveToDefault;
        public event Action<bool> CompletedStateChanged;
        public event Action<bool> FirstInFlowChanged;

        public EditingStepModel(LevelEditorState state)
        {
            State = state;
        }
        
        public void MoveBackAction() => MoveBack?.Invoke();
        public void MoveNextAction() => MoveNext?.Invoke();
        public void MoveToDefaultAction() => MoveToDefault?.Invoke();
    }
}