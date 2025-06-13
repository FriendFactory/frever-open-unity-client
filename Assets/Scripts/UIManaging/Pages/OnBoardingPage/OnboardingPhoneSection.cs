using System;
using System.Collections;
using System.Text.RegularExpressions;
using AdvancedInputFieldPlugin;
using Bridge.Authorization.Models;
using Extensions;
using Modules.SignUp;
using TMPro;
using UIManaging.PopupSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.OnBoardingPage
{
    internal sealed class OnboardingPhoneSection : MonoBehaviour, ICredentialsProvider
    {
        [SerializeField] private Button _validationErrorButton;
        [SerializeField] private TMP_Text _validationError;
        [Space]
        [SerializeField] private AdvancedInputField _phoneInputField;
        [SerializeField] private Button _phoneInputButton;
        [SerializeField] private Button _countryCodeButton;
        [SerializeField] private TMP_Text _countryCode;

        [Inject] private PopupManager _popupManager;

        private CountryCodeProvider _countryCodeProvider;
        private PhoneNumberCredentials _credentials = new PhoneNumberCredentials();
        private CountryCode[] _countryCodes;

        public string Number => _credentials.PhoneNumber;
        public ICredentials Credentials => _credentials;
        public event Action ValidationErrorClicked;
        public event Action<bool> InputValidated;
        
        private void OnEnable()
        {
            _validationErrorButton.onClick.AddListener(OnValidationErrorButtonClicked);
        }

        private void OnDisable()
        {
            _phoneInputButton.onClick.RemoveAllListeners();
            _countryCodeButton.onClick.RemoveAllListeners();
            _validationErrorButton.onClick.RemoveAllListeners();
        }

        public void Show()
        {
            _countryCodeProvider ??= new CountryCodeProvider();
            
            gameObject.SetActive(true);
            
            _validationError.SetActive(false);
            
            _phoneInputField.OnValueChanged.AddListener(OnValueChanged);
            
            _countryCodeButton.onClick.AddListener(OnCountryCodeButtonClicked);
            _phoneInputButton.onClick.AddListener(OnInputFieldButtonClicked);
            _countryCodeProvider.CountryCodeChanged += OnCountryCodeChanged;
            _countryCode.text = $"+{_countryCodeProvider.DefaultCountryCode.DialCode}";

            StartCoroutine(DelayedSelectCoroutine());
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            
            _phoneInputField.OnValueChanged.RemoveAllListeners();
            _phoneInputField.Clear();
            
            _phoneInputButton.onClick.RemoveAllListeners();
            _countryCodeButton.onClick.RemoveAllListeners();
            
            if (_countryCodeProvider is null) return;
            _countryCodeProvider.CountryCodeChanged -= OnCountryCodeChanged;
        }

        public void ShowValidationError(string message)
        {
            _validationError.SetActive(true);
            _validationError.text = message;
        }

        private void OnCountryCodeButtonClicked()
        {
            _countryCodeProvider.ShowPicker(_popupManager);
        }

        private void OnInputFieldButtonClicked()
        {
            _phoneInputField.Select();
        }
        
        private void OnCountryCodeChanged(int code)
        {
            _countryCode.text = $"+{code}";
        }

        private void OnValueChanged(string value)
        {
            var phoneNumber = $"{_countryCode.text}{value}";
            phoneNumber = Regex.Replace(phoneNumber, "[^a-zA-Z0-9 +]", "");
            _credentials.PhoneNumber = phoneNumber;
            var ok = !string.IsNullOrEmpty(value) && value.Length >= 7;
            InputValidated?.Invoke(ok);
        }

        private void OnValidationErrorButtonClicked()
        {
            ValidationErrorClicked?.Invoke();
        }

        private IEnumerator DelayedSelectCoroutine()
        {
            yield return null;
            
            _phoneInputField.Select();
        }
    }
}