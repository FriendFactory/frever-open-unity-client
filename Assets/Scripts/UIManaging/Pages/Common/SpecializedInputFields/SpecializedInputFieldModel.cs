using System;
using TMPro;
using UIManaging.Common.InputFields;

namespace UIManaging.Pages.Common.RegistrationInputFields
{
    public sealed class SpecializedInputFieldModel
    {
        public string InitialText { get; set; } = string.Empty;
        public string PlaceHolderText { get; set; } = string.Empty;
        public TMP_InputField.InputType InputType { get; set; }
        public TMP_InputField.ContentType ContentType { get; set; }
        public int CharacterLimit { get; set; }
        public Action<string> OnValueChanged { get; set; }
        public Action OnKeyboardSubmit { get; set; }
        public Action<KeyboardStatus> OnKeyboardStatusChanged { get; set; }
    }
}