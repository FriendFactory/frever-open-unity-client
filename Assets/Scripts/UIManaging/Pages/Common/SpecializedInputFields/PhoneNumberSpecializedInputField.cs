using UIManaging.Pages.Common.RegistrationInputFields.CountryCodePickers;
using UnityEngine;

namespace UIManaging.Pages.Common.RegistrationInputFields
{
    internal sealed class PhoneNumberSpecializedInputField : SpecializedInputFieldBase
    {
        [SerializeField] private CountryCodePickerBase _countryCodePickeriOS;
        [SerializeField] private CountryCodePickerBase _countryCodePickerEditor;
        
        private CountryCodePickerBase _countryCodePicker;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public override SpecializationType Type => SpecializationType.PhoneNumber;
        protected override bool OpenKeyboardOnDisplay => true;

        //---------------------------------------------------------------------
        // Messagees
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _countryCodePicker = InstantiateCountryCodePicker();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public override void Display()
        {
            base.Display();
            
            _countryCodePicker.gameObject.SetActive(true);
        }
        
        public override void Hide()
        {
            base.Hide();
            
            _countryCodePicker.gameObject.SetActive(false);
        }

        public override string ApplyTextAlterations()
        {
            var countryCode = _countryCodePicker.GetSelectedCountryCode();
            var result = InputField.Text.Insert(0,countryCode);
            return result;
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void SetInitialText(string text)
        {
            InputField.Text = _countryCodePicker.RemoveDialCode(text);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private CountryCodePickerBase InstantiateCountryCodePicker()
        {
            var countryCodePickerPrefab = Application.platform == RuntimePlatform.IPhonePlayer
                ? _countryCodePickeriOS
                : _countryCodePickerEditor;

            var root = GetComponentInParent<Canvas>().rootCanvas;
            
            return Instantiate(countryCodePickerPrefab, root.transform);
        }
    }
}