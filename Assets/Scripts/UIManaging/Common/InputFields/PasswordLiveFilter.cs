using System.Text;
using AdvancedInputFieldPlugin;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Common.InputFields
{
    [RequireComponent(typeof(AdvancedInputField))]
    internal sealed class PasswordLiveFilter: LiveDecorationFilter 
    {
        [SerializeField] private Toggle _passwordVisibilityToggle;

        private AdvancedInputField _inputField;
        private StringBuilder _stringBuilder;
        private string _lastText;
        private bool _visibilityChanged;

        private bool ShowPassword => !_passwordVisibilityToggle.isOn;
        private AdvancedInputField InputField => _inputField ? _inputField : _inputField = GetComponent<AdvancedInputField>();

        private void OnEnable()
        {
            _passwordVisibilityToggle.onValueChanged.AddListener(OnPasswordVisibilityChanged);
        }
        
        private void OnDisable()
        {
            _passwordVisibilityToggle.onValueChanged.RemoveListener(OnPasswordVisibilityChanged);
        }

        public override string ProcessText(string text, int caretPosition)
        {
            _lastText = text;

            if (string.IsNullOrEmpty(_lastText)) return _lastText;

            return GetProcessedText(_lastText);
        }

        public override int DetermineProcessedCaret(string text, int caretPosition, string processedText) => caretPosition;

        public override int DetermineCaret(string text, string processedText, int processedCaretPosition) => processedCaretPosition;

        public override bool UpdateFilter(out string processedText, bool lastUpdate = false)
        {
            if (!_visibilityChanged) return base.UpdateFilter(out processedText, lastUpdate);
            
            _visibilityChanged = false;
            processedText = GetProcessedText(_lastText);
                
            return true;
        }

        private string GetProcessedText(string text)
        {
            if (ShowPassword) return text;
            
            _stringBuilder ??= new StringBuilder();

            _stringBuilder.Clear();

            var length = text.Length;
            for (var i = 0; i < length; i++)
            {
                _stringBuilder.Append('*');
            }

            return _stringBuilder.ToString();
        }

        private void OnPasswordVisibilityChanged(bool _)
        {
            if (InputField.Selected)
            {
                _visibilityChanged = true;
                return;
            }

            // filters work only when input field is selected, so, we need to update processed text manually 
            // when password visibility is changed for unselected input field
            var processedText = GetProcessedText(_lastText);
            
            // do not know how to achieve the same result w/o exposing the internal Engine method
            InputField.Engine.SetProcessedText(processedText);
        }
    }
}