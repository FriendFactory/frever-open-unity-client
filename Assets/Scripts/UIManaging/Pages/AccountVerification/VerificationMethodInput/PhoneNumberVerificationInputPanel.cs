using System.Text.RegularExpressions;
using Modules.SignUp;
using TMPro;
using UIManaging.Pages.OnBoardingPage;
using UIManaging.PopupSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.AccountVerification.VerificationMethodInput
{
    internal sealed class PhoneNumberVerificationInputPanel: VerificationMethodInputPanel
    {
        [SerializeField] private Button _countryCodeButton;
        [SerializeField] private TMP_Text _countryCode;
        
        [Inject] private PopupManager _popupManager;
        
        private CountryCodeProvider _countryCodeProvider;
        private CountryCode[] _countryCodes;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            _countryCodeButton.onClick.AddListener(OnCountryCodeButtonClicked);
            
            _countryCodeProvider ??= new CountryCodeProvider();
            _countryCode.text = $"+{_countryCodeProvider.DefaultCountryCode.DialCode}";

            _countryCodeProvider.CountryCodeChanged += OnCountryCodeChanged;
        }

        protected override void BeforeCleanUp()
        {
            base.BeforeCleanUp();
            
            _countryCodeButton.onClick.RemoveListener(OnCountryCodeButtonClicked);
            
            _countryCodeProvider.CountryCodeChanged -= OnCountryCodeChanged;
            
            _countryCodeProvider.HidePicker(_popupManager);
        }

        protected override void OnValueChanged(string value)
        {
            var phoneNumber = $"{_countryCode.text}{value}";
            phoneNumber = Regex.Replace(phoneNumber, "[^a-zA-Z0-9 +]", "");

            base.OnValueChanged(phoneNumber);
        }

        public override void Select()
        {
            _inputField.Select();
        }

        private void OnCountryCodeButtonClicked()
        {
            _countryCodeProvider.ShowPicker(_popupManager);
        }
        
        private void OnCountryCodeChanged(int code)
        {
            _countryCode.text = $"+{code}";
        }
    }
}