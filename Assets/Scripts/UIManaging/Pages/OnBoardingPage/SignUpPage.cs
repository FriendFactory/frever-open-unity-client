using System;
using System.Threading.Tasks;
using Bridge.Authorization.Models;
using Extensions;
using Modules.Amplitude;
using Modules.SignUp;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Localization;
using UIManaging.PopupSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.OnBoardingPage
{
    public class SignUpPage : GenericPage<SignUpArgs>
    {
        [SerializeField] private TextMeshProUGUI _phoneDescription;
        [SerializeField] private TextMeshProUGUI _emailDescription;
        [SerializeField] private TextMeshProUGUI _passwordDescription;
        [SerializeField] private TextMeshProUGUI _continueText;
        [SerializeField] private Button _backButton;
        [Space]
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _emailButton;
        [SerializeField] private Button _phoneButton;
        [SerializeField] private Button _appleButton;
        [SerializeField] private Button _googleButton;
        [SerializeField] private Button _usernameButton;
        
        [Header("Phone")] 
        [SerializeField] private OnboardingPhoneSection _phoneSection;

        [Header("Email")] 
        [SerializeField] private OnboardingEmailSection _emailSection;

        [Header("Password")] 
        [SerializeField] private OnboardingPasswordSection _passwordSection;
        
        [Inject] private ISignUpService _signUpService;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private OnBoardingLocalization _localization;
        [Inject] private AmplitudeManager _amplitudeManager;

        private ICredentialsProvider _currentProvider;
        
        public override PageId Id => PageId.SignUp;

        private void Awake()
        {
            _continueButton.interactable = false;
        }
        
        private void OnEnable()
        {
            _backButton.onClick.AddListener(OnBackButtonClicked);
            
            _continueButton.onClick.AddListener(OnContinueButtonClicked);
            _phoneButton.onClick.AddListener(OnPhoneButtonClicked);
            _emailButton.onClick.AddListener(OnEmailButtonClicked);
            _appleButton.onClick.AddListener(OnAppleButtonClicked);
            _googleButton.onClick.AddListener(OnGoogleButtonClicked);
            _usernameButton.onClick.AddListener(OnUsernameButtonClicked);
        
            _currentProvider = _passwordSection;
        }
        
        private void OnDisable()
        {
            _backButton.onClick.RemoveAllListeners();
            
            _continueButton.onClick.RemoveAllListeners();
            _phoneButton.onClick.RemoveAllListeners();
            _emailButton.onClick.RemoveAllListeners();
            _appleButton.onClick.RemoveAllListeners();
            _googleButton.onClick.RemoveAllListeners();
            _usernameButton.onClick.RemoveAllListeners();

            _phoneSection.ValidationErrorClicked -= OnValidationErrorClicked;
            _emailSection.ValidationErrorClicked -= OnValidationErrorClicked;
            _phoneSection.InputValidated -= OnInputValidated;
            _emailSection.InputValidated -= OnInputValidated;
            _passwordSection.InputValidated -= OnInputValidated;
        }

        protected override void OnInit(PageManager pageManager)
        {
        }
        
        protected override void OnDisplayStart(SignUpArgs args)
        {
            base.OnDisplayStart(args);
            
            args.MoveNextFailed += OnMoveNextFailed;
            
            _phoneDescription.text = string.Format(_localization.SignUpPhoneDescription, _signUpService.SelectedUserName);
            _emailDescription.text = string.Format(_localization.SignUpEmailDescription, _signUpService.SelectedUserName);
            _passwordDescription.text = string.Format(_localization.SignUpPasswordDescription, _signUpService.SelectedUserName);
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            OpenPageArgs.MoveNextFailed -= OnMoveNextFailed;
            
            base.OnHidingBegin(onComplete);
        }

        private void OnEmailButtonClicked()
        {
            _currentProvider = _emailSection;
            _continueButton.interactable = false;
            _continueText.text = _localization.SignUpEmailButton;
            
#if UNITY_ANDROID
            _appleButton.SetActive(false);
            _googleButton.SetActive(true);
#elif UNITY_IOS
            _appleButton.SetActive(true);
            _googleButton.SetActive(false);
#endif
            
            _emailButton.SetActive(false);
            _phoneButton.SetActive(true);
            _usernameButton.SetActive(true);
            
            _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.CONTINUE_WITH_EMAIL);

            _emailSection.Show();
            _emailSection.InputValidated += OnInputValidated;
            _phoneSection.Hide();
            _phoneSection.InputValidated -= OnInputValidated;
            _passwordSection.Hide();
            _passwordSection.InputValidated -= OnInputValidated;
        }

        private void OnPhoneButtonClicked()
        {
            _currentProvider = _phoneSection;
            _continueButton.interactable = false;
            _continueText.text = _localization.SignUpPhoneButton;
            
#if UNITY_ANDROID
            _appleButton.SetActive(false);
            _googleButton.SetActive(true);
#elif UNITY_IOS
            _appleButton.SetActive(true);
            _googleButton.SetActive(false);
#endif
            
            _emailButton.SetActive(true);
            _phoneButton.SetActive(false);
            _usernameButton.SetActive(true);
            
            _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.CONTINUE_WITH_PHONENUMBER);

            _emailSection.Hide();
            _emailSection.InputValidated -= OnInputValidated;
            _phoneSection.Show();
            _phoneSection.InputValidated += OnInputValidated;
            _passwordSection.Hide();
            _passwordSection.InputValidated -= OnInputValidated;
        }
        
        private void OnUsernameButtonClicked()
        {
            _currentProvider = _passwordSection;
            _continueButton.interactable = false;
            _continueText.text = _localization.SignUpPasswordButton;
            
#if UNITY_ANDROID
            _appleButton.SetActive(false);
            _googleButton.SetActive(true);
#elif UNITY_IOS
            _appleButton.SetActive(true);
            _googleButton.SetActive(false);
#endif
            
            _emailButton.SetActive(true);
            _phoneButton.SetActive(true);
            _usernameButton.SetActive(false);

            _emailSection.Hide();
            _emailSection.InputValidated -= OnInputValidated;
            _phoneSection.Hide();
            _phoneSection.InputValidated -= OnInputValidated;
            _passwordSection.Show();
            _passwordSection.InputValidated += OnInputValidated;
        }

        private void OnAppleButtonClicked()
        {
            _signUpService.RequestAppleIdCredentials(OnSuccess, OnMoveNextFailed);
            
            _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.CONTINUE_WITH_APPLE);

            void OnSuccess(AppleAuthCredentials credentials)
            {
                OpenPageArgs.ValidCredentialsProvided(credentials);
            }
        }

        private void OnGoogleButtonClicked()
        {
            _signUpService.RequestGooglePlayCredentials(OnSuccess, _ => OnMoveNextFailed());
            
            _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.CONTINUE_WITH_GOOGLE);

            void OnSuccess(GoogleAuthCredentials credentials)
            {
                OpenPageArgs.ValidCredentialsProvided(credentials);
            }
        }

        private async void OnContinueButtonClicked()
        {
            if (ReferenceEquals(_currentProvider, _phoneSection))
            {
                await ValidatePhoneNumberAndContinue();
                return;
            }
            
            if (ReferenceEquals(_currentProvider, _emailSection))
            {
                await ValidateEmailAndContinue();
                return;
            }
            
            if (ReferenceEquals(_currentProvider, _passwordSection))
            {
                await ValidatePasswordAndContinue();
            }
        }

        private async Task ValidateEmailAndContinue()
        {
            _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.VERIFY_EMAIL);
            
            var result = await _signUpService.ValidateEmail(_emailSection.Credentials as EmailCredentials);
            if (result.IsValid)
            {
                OpenPageArgs.ValidCredentialsProvided?.Invoke(_emailSection.Credentials);
                return;
            }

            _emailSection.ShowValidationError(result.ReasonPhrase);
            _continueButton.interactable = false;
        }

        private async Task ValidatePhoneNumberAndContinue()
        {
            _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.VERIFY_PHONENUMBER);
            
            var result = await _signUpService.ValidatePhoneNumber(_phoneSection.Credentials as PhoneNumberCredentials);
            if (result.IsValid)
            {
                OpenPageArgs.ValidCredentialsProvided?.Invoke(_phoneSection.Credentials);
                return;
            }
            
            _phoneSection.ShowValidationError(result.ReasonPhrase);
            _continueButton.interactable = false;
        }

        private async Task ValidatePasswordAndContinue()
        {
            var credentials = _passwordSection.Credentials as UsernameAndPasswordCredentials;
            var password = credentials?.Password;
            var nickname = _signUpService.SelectedUserName;
            var result = await _signUpService.ValidatePassword(password, nickname);
            if (result.IsValid)
            {
                OpenPageArgs.ValidCredentialsProvided?.Invoke(_passwordSection.Credentials);
                return;
            }
            
            _passwordSection.ShowValidationError(result.ReasonPhrase);
            _continueButton.interactable = false;
        }
        
        private void OnInputValidated(bool ok)
        {
            _continueButton.interactable = ok;
        }

        private void OnBackButtonClicked()
        {
            OpenPageArgs.MoveBackRequested?.Invoke();
        }

        private void OnValidationErrorClicked()
        {
            _popupManagerHelper.ShowReversedDialogPopup(_localization.GoToLogInTitle, _localization.GoToLogInDescription,
                                                _localization.GoToLogInNoButton, () => { }, _localization.GoToLogInYesButton, 
                                                () => OpenPageArgs.MoveToSignInRequested?.Invoke(_currentProvider.Credentials), 
                                                true);
        }

        private void OnMoveNextFailed()
        {
            if (ReferenceEquals(_currentProvider, _phoneSection))
            {
                _phoneSection.ShowValidationError(_localization.SignUpAppleError);
            }
            else if(ReferenceEquals(_currentProvider, _emailSection))
            {
                _emailSection.ShowValidationError(_localization.SignUpAppleError);
            }
            else if(ReferenceEquals(_currentProvider, _passwordSection))
            {
                _passwordSection.ShowValidationError(_localization.SignUpAppleError);
            }

            _continueButton.interactable = false;
        }
    }
}