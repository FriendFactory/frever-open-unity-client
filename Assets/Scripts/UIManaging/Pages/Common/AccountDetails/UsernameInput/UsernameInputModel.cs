using System;

namespace UIManaging.Pages.EditUsername
{
    public enum ValidationEventState
    {
        Started = 0,
        Finished = 1,
    }
    
    public class UsernameInputModel
    {
        
        public string Input
        {
            get => _input;
            set => SetInput(value, true);
        }
        
        public bool IsValid { get; set; }

        public event Action<string> InputChanged;
        public event Action<string> InputRandomized;
        public event Action<ValidationEventState> ValidationEvent;
        public event Action<UsernameValidationResult> InputValidated;

        private string _input;
        
        public void SetInput(string value, bool notify = false)
        {
            if (string.Equals(_input, value)) return;
            
            _input = value;

            if (notify)
            {
                InputChanged?.Invoke(_input);
            }
        }

        public void OnValidationEvent(ValidationEventState state) => ValidationEvent?.Invoke(state);
        public void OnInputValidated(UsernameValidationResult result) => InputValidated?.Invoke(result);
        public void OnInputRandomized(string username) => InputRandomized?.Invoke(username);
    }
}