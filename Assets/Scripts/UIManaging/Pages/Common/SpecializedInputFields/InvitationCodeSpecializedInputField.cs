using TMPro;
using UnityEngine;

namespace UIManaging.Pages.Common.RegistrationInputFields
{
    internal sealed class InvitationCodeSpecializedInputField : SpecializedInputFieldBase
    {
        [SerializeField] private TextMeshProUGUI[] _codeFieldTexts;
        public override SpecializationType Type => SpecializationType.InvitationCode;
        
        private Color _originCaretColor;
        protected override bool OpenKeyboardOnDisplay => true;

        private void Awake()
        {
            ClearAllTextFields();
            _originCaretColor = InputField.CaretColor;
        }
        

        protected override void OnValueChanged(string text)
        {
            base.OnValueChanged(text);
            
            ClearAllTextFields();
            
            for (var i = 0; i < text.Length; i++)
            {
                _codeFieldTexts[i].text = text[i].ToString().ToUpper();
            }

            InputField.CaretColor = text.Length == _codeFieldTexts.Length ? Color.clear : _originCaretColor;
        }

        private void ClearAllTextFields()
        {
            foreach (var textField in _codeFieldTexts)
            {
                textField.text = string.Empty;
            }
        }
    }
}
