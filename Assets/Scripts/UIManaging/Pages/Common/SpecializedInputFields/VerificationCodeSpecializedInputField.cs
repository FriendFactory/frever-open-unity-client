using TMPro;
using UnityEngine;

namespace UIManaging.Pages.Common.RegistrationInputFields
{
    internal sealed class VerificationCodeSpecializedInputField : SpecializedInputFieldBase
    {
        [SerializeField] private TextMeshProUGUI[] _texts;
        
        private Color _originCaretColor;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public override SpecializationType Type => SpecializationType.Authentication;
        protected override bool OpenKeyboardOnDisplay => true;
        protected override bool AllowPaste => true;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            base.OnInitialized();
            ClearAllTextFields();
            _originCaretColor = InputField.CaretColor;
        }

        protected override void OnValueChanged(string text)
        {
            if (text.Length > _texts.Length) return;
            
            base.OnValueChanged(text);
            
            ClearAllTextFields();
            
            for (var i = 0; i < text.Length; i++)
            {
                _texts[i].text = text[i].ToString();
            }

            InputField.CaretColor = text.Length == _texts.Length ? Color.clear : _originCaretColor;
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void ClearAllTextFields()
        {
            foreach (var textField in _texts)
            {
                textField.text = string.Empty;
            }
        }
    }
}