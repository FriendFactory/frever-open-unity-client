using System;
using AdvancedInputFieldPlugin;
using Bridge.Authorization.Models;
using Common;
using Extensions;
using Modules.SignUp;
using TMPro;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.OnBoardingPage
{
    public class UsernameLoginSection : MonoBehaviour, ICredentialsProvider
    {
        [SerializeField] private AdvancedInputField _username;
        [SerializeField] private AdvancedInputField _password;
        [SerializeField] private TMP_Text _validationError;

        [Inject] private ISignInService _signInService;
    
        private UsernameAndPasswordCredentials _credentials;
    
        public string Username => _credentials.Username;
        public string Password => _credentials.Password;
        public ICredentials Credentials => _credentials;
        public event Action<bool> InputValidated;
    
        public void Show()
        {
            gameObject.SetActive(true);
            _credentials = new UsernameAndPasswordCredentials { Email = string.Empty };
        
            _username.OnValueChanged.AddListener(OnUsernameValueChanged);
            _password.OnValueChanged.AddListener(OnPasswordValueChanged);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _validationError.SetActive(false);
        
            _username.OnValueChanged.RemoveAllListeners();
            _password.OnValueChanged.RemoveAllListeners();
        
            _username.Clear();
            _password.Clear();
        }

        public void ShowValidationError(string message)
        {
            _validationError.text = message;
            _validationError.SetActive(true);
        }

        private void OnUsernameValueChanged(string value)
        {
            _credentials.Username = value;
        
            var ok = ValidateCredentials();
            InputValidated?.Invoke(ok);
        }

        private void OnPasswordValueChanged(string value)
        {
            _credentials.Password = value;

            var ok = ValidateCredentials();
            InputValidated?.Invoke(ok);
        }

        private bool ValidateCredentials()
        {
            if (string.IsNullOrEmpty(_credentials.Username)) return false;
            if (string.IsNullOrEmpty(_credentials.Password) || _credentials.Password.Length < Constants.Onboarding.MIN_PASSWORD_LENGTH)
            {
                return false;
            }

            return true;
        }
    }
}
