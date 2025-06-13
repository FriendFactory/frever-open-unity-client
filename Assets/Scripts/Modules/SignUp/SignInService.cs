using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bridge;
using Bridge.Authorization.Models;
using Bridge.Results;
using Common;
using Modules.Amplitude;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Navigation.Core;
using UIManaging.Localization;
using UIManaging.Pages.Common.UserLoginManagement;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.NoConnectionPage;
using UIManaging.Pages.UpdateAppPage;
using UIManaging.PopupSystem;
using UnityEngine;
using Zenject;

namespace Modules.SignUp
{
    public interface ISignInService
    {
        void LoginWithAppleCredentials(Action onLoginSuccess, Action onLoginFail);
        void LoginWithGoogleCredentials(Action onLoginSuccess, Action onLoginFail);
        void SetCredentials(ICredentials credentials);
        Task<Result> LoginUser();
        void SetVerificationCode(string value);
        Task RequestVerificationCode();
        string VerificationCodeDescription();
        Task<ValidationResult> ValidateEmail(string email);
        Task<ValidationResult> ValidatePhoneNumber(string phoneNumber);
        Task<ValidationResult> ValidateUsername(string username);
        Task<bool> ParentEmailAvailable(string username);
    }

    [UsedImplicitly]
    public sealed class SignInService : ISignInService
    {
        [Inject] private ServerBridge _bridge;
        [Inject] private PopupManager _popupManager;
        [Inject] private PageManager _pageManager;
        [Inject] private AmplitudeManager _amplitudeManager;
        [Inject] private UserAccountManager _userAccountManager;
        [Inject] private LocalUserDataHolder _localUser;
        [Inject] private IDataFetcher _dataFetcher;
        [Inject] private OnBoardingLocalization _localization;
        [Inject] private OnboardingServerErrorLocalization _errorLocalization;

        private ICredentials _credentials;
        private AppleIdCredentialsProvider _appleIdCredentialsProvider;
        private GooglePlayCredentialsProvider _googlePlayCredentialsProvider;

        public void LoginWithAppleCredentials(Action onLoginSuccess, Action onLoginFail)
        {
            if (_appleIdCredentialsProvider is null)
            {
                _appleIdCredentialsProvider = new AppleIdCredentialsProvider();
            }
            
            _appleIdCredentialsProvider.CredentialsRequestCompleted += OnSuccess;
            _appleIdCredentialsProvider.RequestCredentials();

            async void OnSuccess(AppleAuthCredentials appleAuthCredentials)
            {
                SetCredentials(appleAuthCredentials);
                
                var result = await LoginUser();
                
                if (result.IsError)
                {
                    Debug.LogError(result.ErrorMessage);
                    onLoginFail?.Invoke();
                    return;
                }
                
                onLoginSuccess?.Invoke();
            }
        }
        
        public void LoginWithGoogleCredentials(Action onLoginSuccess, Action onLoginFail)
        {
            if (_googlePlayCredentialsProvider is null)
            {
                _googlePlayCredentialsProvider = new GooglePlayCredentialsProvider();
            }
            
            _googlePlayCredentialsProvider.CredentialsRequestCompleted += OnSuccess;
            _googlePlayCredentialsProvider.RequestCredentials();

            async void OnSuccess(GoogleAuthCredentials googleAuthCredentials)
            {
                SetCredentials(googleAuthCredentials);
                
                var result = await LoginUser();
                
                if (result.IsError)
                {
                    Debug.LogError(result.ErrorMessage);
                    onLoginFail?.Invoke();
                    return;
                }
                
                onLoginSuccess?.Invoke();
            }
        }
        
        public void SetCredentials(ICredentials credentials)
        {
            _credentials = credentials;
        }

        public async Task<Result> LoginUser()
        {
            _amplitudeManager.UploadEventsNow();
            
            var compLastEnv = await _bridge.GetEnvironmentCompatibilityData(_bridge.Environment);

            if (compLastEnv.IsError)
            {
                _pageManager.MoveNext(PageId.NoConnectionPage, new NoConnectionPageArgs());
            }
                
            if(!compLastEnv.IsCompatibleWithBridge)
            {
                _pageManager.MoveNext(PageId.UpdateAppPage, new UpdateAppPageArgs());
            }
            
            string error = null;
            var isLoggedIn = await _userAccountManager.LogInAsync(_credentials, true, targetError => error = targetError);
            if (!isLoggedIn)
            {
                return new ErrorResult(error);
            }
            
            if (_credentials is PhoneNumberCredentials)
            {
                _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.PHONENUMBER_VALIDATED);
            }

            return new SuccessResult();
        }

        public void SetVerificationCode(string value)
        {
            switch (_credentials)
            {
                case null:
                    Debug.LogError("Credentials not set");
                    break;
                case EmailCredentials emailCredentials:
                    emailCredentials.VerificationCode = value;
                    break;
                case PhoneNumberCredentials phoneNumberCredentials:
                    phoneNumberCredentials.VerificationCode = value;
                    break;
                case AppleAuthCredentials appleAuthCredentials :
                    break;
            }
        }

        public async Task RequestVerificationCode()
        {
            switch (_credentials)
            {
                case null:
                    Debug.LogError("Credentials not set");
                    return;
                case EmailCredentials emailCredentials:
                {
                    var result = await _bridge.RequestEmailVerificationCode(emailCredentials.Email);
                
                    if (result.IsError) Debug.LogError(result.ErrorMessage);
                    break;
                }
                case PhoneNumberCredentials phoneNumberCredentials:
                {
                    var result = await _bridge.RequestPhoneNumberVerificationCode(phoneNumberCredentials.PhoneNumber);
                    
                    if (result.IsError) Debug.LogError(result.ErrorMessage);
                    break;
                }
                case AppleAuthCredentials appleCredentials:
                {
                    throw new NotImplementedException();
                }
            }
        }

        public string VerificationCodeDescription()
        {
            switch (_credentials)
            {
                case null:
                    Debug.LogError("Credentials not set");
                    return string.Empty;
                case EmailCredentials emailCredentials:
                    return string.Format(_localization.VerificationCodeEmailDescription, emailCredentials.Email);
                case PhoneNumberCredentials phoneNumberCredentials:
                    return string.Format(_localization.VerificationCodePhoneDescription, phoneNumberCredentials.PhoneNumber);
                case AppleAuthCredentials appleAuthCredentials:
                    throw new NotImplementedException();
            }

            return string.Empty;
        }

        public async Task<ValidationResult> ValidateEmail(string email)
        {
            var validationModel = new ValidationModel
            {
                Email = email,
                PhoneNumber = null,
                UserName = null,
            };

            var result = await _bridge.ValidateLoginCredentials(validationModel);

            if (result.Data.IsValid) _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.EMAIL_VALIDATED);


            return new ValidationResult
            {
                IsValid = result.Data.IsValid,
                ReasonPhrase = _localization.EmailLoginValidationOverrideMessage, 
                RequirementFailed = new Dictionary<RequirementType, bool>()
            };
        }

        public async Task<ValidationResult> ValidatePhoneNumber(string phoneNumber)
        {
            var validationModel = new ValidationModel
            {
                PhoneNumber = phoneNumber,
                Email = null,
                UserName = null,
            };

            var result = await _bridge.ValidateLoginCredentials(validationModel);

            if (result.Data.IsValid) _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.EMAIL_VALIDATED);

            return new ValidationResult
            {
                IsValid = result.Data.IsValid,
                ReasonPhrase = result.Data.ErrorCode == OnboardingServerErrorLocalization.PhoneNumberFormatInvalid 
                    ? _errorLocalization.GetLocalized(OnboardingServerErrorLocalization.PhoneNumberFormatInvalid)
                    : _localization.PhoneLoginValidationOverrideMessage, 
                RequirementFailed = new Dictionary<RequirementType, bool>()
            };
        }

        public async Task<ValidationResult> ValidateUsername(string username)
        {
            var validationModel = new ValidationModel
            {
                UserName = username,
                PhoneNumber = null,
                Email = null,
            };

            var result = await _bridge.ValidateLoginCredentials(validationModel);
            return new ValidationResult
            {
                IsValid = result.Data.IsValid,
                ReasonPhrase = _localization.UsernameContentErrorMessage, 
                RequirementFailed = new Dictionary<RequirementType, bool>()
            };
        }

        public async Task<bool> ParentEmailAvailable(string username)
        {
            var result = await _bridge.CheckIfParentEmailBound(username);
            if (!result.IsSuccess)
            {
                Debug.LogError(result.ErrorMessage);
                return false;
            }

            return result.Model;
        }
    }
}