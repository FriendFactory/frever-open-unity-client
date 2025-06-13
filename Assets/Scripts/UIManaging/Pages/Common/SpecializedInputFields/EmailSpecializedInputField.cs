using AdvancedInputFieldPlugin;
using UIManaging.Common.InputFields;

namespace UIManaging.Pages.Common.RegistrationInputFields
{
    internal sealed class EmailSpecializedInputField : SpecializedInputFieldBase
    {
        public override SpecializationType Type => SpecializationType.Email;
        protected override bool OpenKeyboardOnDisplay => true;

        private void Awake()
        {
        #if ADVANCEDINPUTFIELD_TEXTMESHPRO
            var filter = _inputField.GetComponent<ForceLowerCaseFilter>();
            if (!filter)
            {
                filter = _inputField.gameObject.AddComponent<ForceLowerCaseFilter>();
            }

            _inputField.ContentType = ContentType.EMAIL_ADDRESS;
            _inputField.LiveProcessingFilter = filter;
        #endif    
        }
    }
}
