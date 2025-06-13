using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bridge.Authorization.Models;
using Extensions;
using Modules.Amplitude;
using Modules.SentryManaging;
using Modules.SignUp;
using UIManaging.Animated.Behaviours;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.OnBoardingPage
{
    public sealed class LoginPopupConfiguration : PopupConfiguration
    {
        public Action<LoginPopupResult> OnComplete { get; }
        public ICredentials Credentials { get; }
        
        public LoginPopupConfiguration(Action<LoginPopupResult> onComplete, ICredentials credentials = null) : base(PopupType.Login, null)
        {
            OnComplete = onComplete;
            Credentials = credentials;
        }
    }

    public enum LoginPopupResult
    {
        Close = 0,
        Next = 1,
        NextNoVerification = 2,
        Signup = 3,
    }

    internal sealed class LoginPopup : BasePopup<LoginPopupConfiguration>
    {
        [SerializeField] private List<Button> _closeButtons;
        [SerializeField] private AnimatedFullscreenOverlayBehaviour _animatedBehaviour;

        [Space]
        [SerializeField] private OnboardingPhoneSection _phoneSection;
        [SerializeField] private OnboardingEmailSection _emailSection;
        [SerializeField] private UsernameLoginSection _usernameSection;
        
        [Space]
        [SerializeField] private Button _appleButton;
        [SerializeField] private Button _googleButton;
        [SerializeField] private Button _emailButton;
        [SerializeField] private Button _phoneButton;
        [SerializeField] private Button _usernameButton;

        [Space] 
        [SerializeField] private Button _continueButton;

        [Inject] private ISignInService _signInService;
        [Inject] private AmplitudeManager _amplitudeManager;
        [Inject] private SentryManager _sentryManager;

        private ICredentialsProvider _currentCredentialsProvider;

        private bool _continueButtonClickIsProcessing;  

        private void Awake()
        {
            _appleButton.SetActive(Application.platform == RuntimePlatform.IPhonePlayer);
            _googleButton.SetActive(Application.platform == RuntimePlatform.Android);
        }
        
        private void OnEnable()
        {
            _animatedBehaviour.PlayInAnimation(null);
            _continueButton.onClick.AddListener(OnContinueButtonClicked);
            _continueButton.interactable = false;
            _continueButtonClickIsProcessing = false;
            _closeButtons.ForEach(b => b.onClick.AddListener(OnCloseButtonClicked));
            
            _appleButton.onClick.AddListener(OnAppleButtonClicked);
            _googleButton.onClick.AddListener(OnGoogleButtonClicked);
            _emailButton.onClick.AddListener(OnEmailButtonClicked);
            _phoneButton.onClick.AddListener(OnPhoneButtonClicked);
            _usernameButton.onClick.AddListener(OnUsernameButtonClicked);
            
            _emailSection.InputValidated += OnInputValidated;
            _emailSection.ValidationErrorClicked += OnValidationTextClicked;
            _phoneSection.InputValidated += OnInputValidated;
            _phoneSection.ValidationErrorClicked += OnValidationTextClicked;
            _usernameSection.InputValidated += OnInputValidated;

            if (Configs == null)
            {
                return;
            }
            
            if (Configs.Credentials == null)
            {
                OnUsernameButtonClicked();
                return;
            }
            
            switch (Configs.Credentials)
            {
                case PhoneNumberCredentials:
                    OnPhoneButtonClicked();
                    break;
                case EmailCredentials:
                    OnEmailButtonClicked();
                    break;
                case UsernameAndPasswordCredentials:
                    OnUsernameButtonClicked();
                    break;
            }
        }

        private void OnDisable()
        {
            _continueButton.onClick.RemoveAllListeners();
            _closeButtons.ForEach(b => b.onClick.RemoveAllListeners());
            
            _appleButton.onClick.RemoveAllListeners();
            _emailButton.onClick.RemoveAllListeners();
            _phoneButton.onClick.RemoveAllListeners();
            _usernameButton.onClick.RemoveAllListeners();
            
            _emailSection.InputValidated -= OnInputValidated;
            _emailSection.ValidationErrorClicked -= OnValidationTextClicked;
            _phoneSection.InputValidated -= OnInputValidated;
            _phoneSection.ValidationErrorClicked -= OnValidationTextClicked;
            _usernameSection.InputValidated -= OnInputValidated;
            
            _emailSection.Hide();
            _phoneSection.Hide();
            _usernameSection.Hide();
        }

        protected override void OnConfigure(LoginPopupConfiguration configuration)
        {
            _closeButtons.ForEach(b => b.interactable = true);
        }

        private void OnEmailButtonClicked()
        {
            _emailButton.SetActive(false);           
            _phoneButton.SetActive(true);
            _usernameButton.SetActive(true);

            _currentCredentialsProvider = _emailSection;
            _emailSection.Show();
            _phoneSection.Hide();
            _usernameSection.Hide();
            
            _continueButton.interactable = false;
        }

        private void OnPhoneButtonClicked()
        {
            _phoneButton.SetActive(false);
            _emailButton.SetActive(true);           
            _usernameButton.SetActive(true);

            _currentCredentialsProvider = _phoneSection;
            _phoneSection.Show();
            _emailSection.Hide();
            _usernameSection.Hide();
        }

        private void OnUsernameButtonClicked()
        {
            _usernameButton.SetActive(false);
            _emailButton.SetActive(true);           
            _phoneButton.SetActive(true);

            _currentCredentialsProvider = _usernameSection;
            _usernameSection.Show();
            _emailSection.Hide();
            _phoneSection.Hide();
        }

        private void OnInputValidated(bool isValid)
        {
            _continueButton.interactable = isValid;
        }

        private void OnAppleButtonClicked()
        {
            _signInService.LoginWithAppleCredentials(OnLoginSuccess, OnLoginFailed);

            void OnLoginSuccess()
            {
                Configs.OnComplete?.Invoke(LoginPopupResult.NextNoVerification);
            }
            
            void OnLoginFailed()
            {
                Configs.OnComplete?.Invoke(LoginPopupResult.Signup);
            }
        }

        private void OnGoogleButtonClicked()
        {
            _signInService.LoginWithGoogleCredentials(OnLoginSuccess, OnLoginFailed);

            void OnLoginSuccess()
            {
                Configs.OnComplete?.Invoke(LoginPopupResult.NextNoVerification);
            }
            
            void OnLoginFailed()
            {
                Configs.OnComplete?.Invoke(LoginPopupResult.Signup);
            }
        }
        
        private void OnCloseButtonClicked()
        {
            _animatedBehaviour.PlayOutAnimation(OnOutComplete);
            _closeButtons.ForEach(b => b.interactable = false);

            void OnOutComplete()
            {
                Configs.OnComplete?.Invoke(LoginPopupResult.Close);
            }
        }

        private async void OnContinueButtonClicked()
        {
            if (_continueButtonClickIsProcessing)
            {
                return;
            }
            _continueButtonClickIsProcessing = true;
            if (!await ValidateCredentials())
            {
                _continueButton.interactable = false;
                _continueButtonClickIsProcessing = false;
                return;
            }
            
            _signInService.SetCredentials(_currentCredentialsProvider.Credentials);

            Configs.OnComplete?.Invoke(_currentCredentialsProvider.Credentials is UsernameAndPasswordCredentials 
                                           ? LoginPopupResult.NextNoVerification : LoginPopupResult.Next);
            _continueButtonClickIsProcessing = false;
        }

        private async Task<bool> ValidateCredentials()
        {
            if (_currentCredentialsProvider is OnboardingEmailSection)
            {
                var emailResult = await _signInService.ValidateEmail(_emailSection.Email);
                if (!emailResult.IsValid)
                {
                    _emailSection.ShowValidationError(emailResult.ReasonPhrase);
                }
                
                return emailResult.IsValid;
            }

            if (_currentCredentialsProvider is UsernameLoginSection)
            {
                var usernameResult = await _signInService.ValidateUsername(_usernameSection.Username);
                
                if (!usernameResult.IsValid)
                {
                    _usernameSection.ShowValidationError(usernameResult.ReasonPhrase);
                }
                else
                {
                    _signInService.SetCredentials(_currentCredentialsProvider.Credentials);
                    
                    var signInResult = await _signInService.LoginUser();

                    if (signInResult.IsSuccess) return true;
                    
                    _usernameSection.ShowValidationError(signInResult.ErrorMessage);
                }

                return false;
            }
            
            var phoneResult = await _signInService.ValidatePhoneNumber(_phoneSection.Number);
            if (!phoneResult.IsValid)
            {
                _phoneSection.ShowValidationError(phoneResult.ReasonPhrase);
            }

            return phoneResult.IsValid;
        }

        private void OnValidationTextClicked()
        {
            Configs.OnComplete?.Invoke(LoginPopupResult.Signup);
        }
    }
}