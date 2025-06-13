using System;
using System.Collections;
using AdvancedInputFieldPlugin;
using Bridge.Authorization.Models;
using Common;
using Extensions;
using TMPro;
using UIManaging.Pages.OnBoardingPage.UI;
using UIManaging.PopupSystem;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.OnBoardingPage
{
    internal sealed class OnboardingPasswordSection : MonoBehaviour, ICredentialsProvider
    {
        [SerializeField] private TMP_Text _validationError;
        [Space]
        [SerializeField] private AdvancedInputField _passwordInputField;
        [SerializeField] private RequirementField _requirement;

        [Inject] private PopupManager _popupManager;

        private readonly UsernameAndPasswordCredentials _credentials = new UsernameAndPasswordCredentials();

        public ICredentials Credentials => _credentials;
        public event Action<bool> InputValidated;

        public void Show()
        {
            gameObject.SetActive(true);
            
            _validationError.SetActive(false);

            _passwordInputField.OnValueChanged.AddListener(OnValueChanged);

            StartCoroutine(DelayedSelectCoroutine());
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            
            _passwordInputField.OnValueChanged.RemoveAllListeners();
            _passwordInputField.Clear();
        }

        public void ShowValidationError(string message)
        {
            _validationError.SetActive(true);
            _validationError.text = message;
        }

        private void OnValueChanged(string value)
        {
            _credentials.Password = value;
            var ok = !string.IsNullOrEmpty(value) && value.Length >= Constants.Onboarding.MIN_PASSWORD_LENGTH;
            _requirement.UpdateStatus(string.IsNullOrEmpty(value) ? UsernameRequirementStatus.Idle 
                                      : ok ? UsernameRequirementStatus.Correct : UsernameRequirementStatus.Incorrect);
            InputValidated?.Invoke(ok);
        }

        private IEnumerator DelayedSelectCoroutine()
        {
            yield return null;
            
            _passwordInputField.Select();
        }
    }
}