using System;
using System.Collections;
using AdvancedInputFieldPlugin;
using Bridge.Authorization.Models;
using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.OnBoardingPage
{
    internal sealed class OnboardingEmailSection : MonoBehaviour, ICredentialsProvider
    {
        [SerializeField] private AdvancedInputField _emailInputField;
        [SerializeField] private TMP_Text _validationError;
        [SerializeField] private Button _validationErrorButton;

        private readonly EmailCredentials _credentials = new EmailCredentials();

        public ICredentials Credentials => _credentials;
        public string Email => _credentials.Email;
        public event Action ValidationErrorClicked;
        public event Action<bool> InputValidated;

        private void OnEnable()
        {
            _validationErrorButton.onClick.AddListener(OnValidationErrorButtonClicked);
        }

        private void OnDisable()
        {
            _validationErrorButton.onClick.RemoveAllListeners();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            
            _emailInputField.OnValueChanged.AddListener(OnValueChanged);

            StartCoroutine(DelayedSelectCoroutine());
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            
            _emailInputField.OnValueChanged.RemoveAllListeners();
            _emailInputField.Clear();
            _validationError.SetActive(false);
        }

        public void ShowValidationError(string message)
        {
            _validationError.SetActive(true);
            _validationError.text = message;
        }

        private void OnValueChanged(string value)
        {
            _credentials.Email = value;
            
            var ok = _credentials.IsEmailValid();
            InputValidated?.Invoke(ok);
        }

        private void OnValidationErrorButtonClicked()
        {
            ValidationErrorClicked?.Invoke();
        }

        private IEnumerator DelayedSelectCoroutine()
        {
            yield return null;
            
            _emailInputField.Select();
        }
    }
}