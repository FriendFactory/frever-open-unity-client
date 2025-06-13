using System.Collections.Generic;
using System.Threading.Tasks;
using Bridge.Authorization.Models;
using Extensions;
using Modules.Amplitude;
using Modules.SignUp;
using TMPro;
using UIManaging.Animated.Behaviours;
using UIManaging.Localization;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Popups;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.OnBoardingPage
{
    internal sealed class AddLoginMethodPopup : BasePopup<AddLoginMethodConfiguration>
    {
        private const string DESCRIPTION_TEXT =
            "Hi <color=#E153C9FF>@{0}</color>, to change your username, add a login method";
        
        [SerializeField] private List<Button> _closeButtons;
        [SerializeField] private AnimatedFullscreenOverlayBehaviour _animatedBehaviour;
        [Space]
        [SerializeField] private OnboardingPhoneSection _phoneSection;
        [SerializeField] private OnboardingEmailSection _emailSection;
        [SerializeField] private OnboardingPasswordSection _passwordSection;
        [Space]
        [SerializeField] private TextMeshProUGUI _description;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _emailButton;
        [SerializeField] private Button _phoneButton;
        [SerializeField] private Button _appleButton;
        [SerializeField] private Button _googleButton;
        [SerializeField] private Button _passwordButton;
        [Space]
        [SerializeField] private GameObject _orText1;
        [SerializeField] private GameObject _orText2;
        
        [Inject] private ISignUpService _signUpService;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private OnBoardingLocalization _localization;
        [Inject] private AmplitudeManager _amplitudeManager;

        private ICredentialsProvider _currentProvider;

        private void Awake()
        {
            ShowEmptyProvider();
        }

        private void OnEnable()
        {
            _animatedBehaviour.PlayInAnimation(null);
            _continueButton.onClick.AddListener(OnContinueButtonClicked);
            _closeButtons.ForEach(b => b.onClick.AddListener(OnCloseButtonClicked));
            
            _appleButton.onClick.AddListener(OnAppleButtonClicked);
            _googleButton.onClick.AddListener(OnGoogleButtonClicked);
            _emailButton.onClick.AddListener(OnEmailButtonClicked);
            _phoneButton.onClick.AddListener(OnPhoneButtonClicked);
            _passwordButton.onClick.AddListener(OnPasswordButtonClicked);
        }

        private void OnDisable()
        {
            _closeButtons.ForEach(b => b.onClick.RemoveAllListeners());

            _appleButton.onClick.RemoveAllListeners();
            _emailButton.onClick.RemoveAllListeners();
            _phoneButton.onClick.RemoveAllListeners();
            _passwordButton.onClick.RemoveAllListeners();
            _continueButton.onClick.RemoveAllListeners();
            
            _phoneSection.ValidationErrorClicked -= OnValidationErrorClicked;
            _emailSection.ValidationErrorClicked -= OnValidationErrorClicked;
            _phoneSection.InputValidated -= OnInputValidated;
            _emailSection.InputValidated -= OnInputValidated;
            _passwordSection.InputValidated -= OnInputValidated;
            
            if (Configs is null)
            {
                return;
            }
            
            Configs.MoveNextFailed -= OnMoveNextFailed;
        }

        private void ShowEmptyProvider()
        {
            _continueButton.SetActive(false);
            _orText1.SetActive(false);
            _orText2.SetActive(true);
#if UNITY_ANDROID
            _appleButton.SetActive(false);
            _googleButton.SetActive(true);
#elif UNITY_IOS
            _appleButton.SetActive(true);
            _googleButton.SetActive(false);
#endif
            _emailButton.SetActive(true);
            _phoneButton.SetActive(true);
            _passwordButton.SetActive(true);
            _emailSection.Hide();
            _emailSection.InputValidated -= OnInputValidated;
            _phoneSection.Hide();
            _phoneSection.InputValidated -= OnInputValidated;
            _passwordSection.Hide();
            _passwordSection.InputValidated -= OnInputValidated;
        }

        private void OnEmailButtonClicked()
        {
            _currentProvider = _emailSection;
            _continueButton.SetActive(true);
            _continueButton.interactable = false;
            
#if UNITY_ANDROID
            _appleButton.SetActive(false);
            _googleButton.SetActive(true);
#elif UNITY_IOS
            _appleButton.SetActive(true);
            _googleButton.SetActive(false);
#endif
            
            _emailButton.SetActive(false);
            _phoneButton.SetActive(true);
            _passwordButton.SetActive(true);
            
            _orText1.SetActive(true);
            _orText2.SetActive(false);
            
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
            _continueButton.SetActive(true);
            _continueButton.interactable = false;
            
#if UNITY_ANDROID
            _appleButton.SetActive(false);
            _googleButton.SetActive(true);
#elif UNITY_IOS
            _appleButton.SetActive(true);
            _googleButton.SetActive(false);
#endif

            _emailButton.SetActive(true);
            _phoneButton.SetActive(false);
            _passwordButton.SetActive(true);
            
            _orText1.SetActive(true);
            _orText2.SetActive(false);

            _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.CONTINUE_WITH_PHONENUMBER);

            _emailSection.Hide();
            _emailSection.InputValidated -= OnInputValidated;
            _phoneSection.Show();
            _phoneSection.InputValidated += OnInputValidated;
            _passwordSection.Hide();
            _passwordSection.InputValidated -= OnInputValidated;
        }

        private void OnPasswordButtonClicked()
        {
            _currentProvider = _passwordSection;
            _continueButton.SetActive(true);
            _continueButton.interactable = false;
            
#if UNITY_ANDROID
            _appleButton.SetActive(false);
            _googleButton.SetActive(true);
#elif UNITY_IOS
            _appleButton.SetActive(true);
            _googleButton.SetActive(false);
#endif

            _emailButton.SetActive(true);
            _phoneButton.SetActive(true);
            _passwordButton.SetActive(false);

            _orText1.SetActive(true);
            _orText2.SetActive(false);

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
                Configs.ValidCredentialsProvided(credentials);
            }
        }

        private void OnGoogleButtonClicked()
        {
            _signUpService.RequestGooglePlayCredentials(OnSuccess, _ => OnMoveNextFailed());
            
            _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.CONTINUE_WITH_GOOGLE);

            void OnSuccess(GoogleAuthCredentials credentials)
            {
                Configs.ValidCredentialsProvided(credentials);
            }
        }

        protected override void OnConfigure(AddLoginMethodConfiguration configuration)
        {
            _closeButtons.ForEach(b => b.interactable = true);
            configuration.MoveNextFailed += OnMoveNextFailed;
            _description.text = string.Format(DESCRIPTION_TEXT, Configs.Username);
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
                Configs.ValidCredentialsProvided?.Invoke(_emailSection.Credentials);
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
                Configs.ValidCredentialsProvided?.Invoke(_phoneSection.Credentials);
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
                Configs.ValidCredentialsProvided?.Invoke(_passwordSection.Credentials);
                return;
            }
            
            _passwordSection.ShowValidationError(result.ReasonPhrase);
            _continueButton.interactable = false;
        }

        private void OnInputValidated(bool ok)
        {
            _continueButton.interactable = ok;
        }

        private void OnValidationErrorClicked()
        {
            _popupManagerHelper.ShowReversedDialogPopup(_localization.GoToLogInTitle, _localization.GoToLogInDescription,
                _localization.GoToLogInNoButton, () => { }, _localization.GoToLogInYesButton, 
                () => Configs.MoveToSignInRequested?.Invoke(_currentProvider.Credentials), 
                true);
        }

        private void OnCloseButtonClicked()
        {
            _animatedBehaviour.PlayOutAnimation(OnOutComplete);
            _closeButtons.ForEach(b => b.interactable = false);

            void OnOutComplete()
            {
                Configs.OnComplete?.Invoke(AddLoginMethodResult.Close);
            }
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
