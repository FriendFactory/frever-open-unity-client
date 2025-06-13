using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bridge;
using Bridge.Authorization.Models;
using Bridge.Models.ClientServer;
using Bridge.Results;
using Common;
using Modules.Amplitude;
using JetBrains.Annotations;
using SA.CrossPlatform.App;
using SA.Foundation.Templates;
using UIManaging.Localization;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using Zenject;

#if UNITY_IOS && !UNITY_EDITOR
using SA.iOS.Foundation;
#endif

namespace Modules.SignUp
{
    [UsedImplicitly]
    public class SignUpService : ISignUpService
    {
        [Inject] private IBridge _bridge;
        [Inject] private LocalUserDataHolder _localUserDataHolder;
        [Inject] private AmplitudeManager _amplitudeManager;
        [Inject] private OnBoardingLocalization _localization;

        private readonly UserRegistrationModel _registrationModel = new UserRegistrationModel();
        private readonly Stack<string> _usernamePool = new Stack<string>();
        private AppleIdCredentialsProvider _appleIdCredentialsProvider;
        private GooglePlayCredentialsProvider _googlePlayCredentialsProvider;

        private CountryInfo _countryInfo;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public string SelectedUserName => _registrationModel.UserName;
        public CountryInfo CountryInfo => _countryInfo;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async Task Initialize()
        {
            await FetchUserCountryInfo();

            _localUserDataHolder.IsNewUser = false;
        }

        public void CleanUp()
        {
            _registrationModel.UserName = string.Empty;
            _localUserDataHolder.BirthDate = default;
        }

        public void SetUserName(string name)
        {
            _registrationModel.UserName = name;
        }

        public void SetCredentials(ICredentials credentials)
        {
            _registrationModel.Credentials = credentials;
        }

        public async Task<ValidationResult> ValidateUsername()
        {
            var model = new ValidationModel
            {
                UserName = _registrationModel.UserName,
                Email = null,
                PhoneNumber = null,
            };
            
            var result = await _bridge.ValidateRegistrationCredentials(model);

            var requirementStatus = new Dictionary<RequirementType, bool>
            {
                { RequirementType.CharacterLimit, result.Data.UserRegistrationErrors.UsernameLengthIncorrect },
                { RequirementType.PersonalInfo, result.Data.UserRegistrationErrors.UsernameModerationFailed },
                { RequirementType.SpecialCharacters, result.Data.UserRegistrationErrors.UsernameContainsForbiddenSymbols },
                { RequirementType.UsernameTaken, result.Data.UserRegistrationErrors.UsernameTaken }
            };
            
            return new ValidationResult
            {
                IsValid = result.Data.IsValid,
                ReasonPhrase = result.Data.ValidationError,
                RequirementFailed = requirementStatus
            };
        }
        
        public int SetBirthdayAndCalculateAge(int month, int year)
        {
            var date = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            _localUserDataHolder.BirthDate = date;
            _registrationModel.BirthDate = date;

            return _localUserDataHolder.GetUserFullYears();
        }

        public async Task<ValidationResult> ValidateEmail(EmailCredentials credentials)
        {
            var validationModel = new ValidationModel
            {
                Email = credentials.Email,
                PhoneNumber = null,
                UserName = null,
            };

            var validationResult = await _bridge.ValidateRegistrationCredentials(validationModel);

            return new ValidationResult
            {
                IsValid = validationResult.Data.IsValid,
                ReasonPhrase = _localization.EmailSignUpValidationOverrideMessage,
                RequirementFailed = new Dictionary<RequirementType, bool>()
            };
        }

        public void RequestAppleIdCredentials(Action<AppleAuthCredentials> onSuccess, Action onFail)
        {
            _appleIdCredentialsProvider = new AppleIdCredentialsProvider();
            _appleIdCredentialsProvider.CredentialsRequestCompleted += OnSuccess;
            _appleIdCredentialsProvider.CredentialsRequestFailed += OnFail;
            _appleIdCredentialsProvider.RequestCredentials();

            void OnSuccess(AppleAuthCredentials credentials)
            {
                _appleIdCredentialsProvider.CredentialsRequestCompleted -= OnSuccess;
                _appleIdCredentialsProvider.CredentialsRequestFailed -= OnFail;
                
                onSuccess?.Invoke(credentials);
            }

            void OnFail()
            {
                _appleIdCredentialsProvider.CredentialsRequestCompleted -= OnSuccess;
                _appleIdCredentialsProvider.CredentialsRequestFailed -= OnFail;
                
                onFail?.Invoke();
            }
        }

        public void RequestGooglePlayCredentials(Action<GoogleAuthCredentials> onSuccess, Action<string> onFail)
        {
            _googlePlayCredentialsProvider = new GooglePlayCredentialsProvider();
            _googlePlayCredentialsProvider.CredentialsRequestCompleted += OnSuccess;
            _googlePlayCredentialsProvider.CredentialsRequestFailed += OnFail;
            _googlePlayCredentialsProvider.RequestCredentials();

            void OnSuccess(GoogleAuthCredentials credentials)
            {
                _googlePlayCredentialsProvider.CredentialsRequestCompleted -= OnSuccess;
                _googlePlayCredentialsProvider.CredentialsRequestFailed -= OnFail;
                
                onSuccess?.Invoke(credentials);
            }

            void OnFail(SA_Error error)
            {
                _googlePlayCredentialsProvider.CredentialsRequestCompleted -= OnSuccess;
                _googlePlayCredentialsProvider.CredentialsRequestFailed -= OnFail;
                
                onFail?.Invoke(error.FullMessage);
            }
        }

        public async Task<ValidationResult> ValidatePhoneNumber(PhoneNumberCredentials credentials)
        {
            var validationModel = new ValidationModel
            {
                PhoneNumber = credentials.PhoneNumber,
                UserName = null,
                Email = null,
            };

            var validationResult = await _bridge.ValidateRegistrationCredentials(validationModel);

            return new ValidationResult
            {
                IsValid = validationResult.Data.IsValid,
                ReasonPhrase = _localization.PhoneSignUpValidationOverrideMessage,
                RequirementFailed = new Dictionary<RequirementType, bool>()
            };
        }

        public async Task RequestVerificationCode()
        {
            switch (_registrationModel.Credentials)
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

        public void SetVerificationCode(string code)
        {
            switch (_registrationModel.Credentials)
            {
                case null:
                    Debug.LogError("Credentials not set");
                    return;
                case EmailCredentials emailCredentials:
                {
                    emailCredentials.VerificationCode = code;
                    break;
                }
                case PhoneNumberCredentials phoneNumberCredentials:
                {
                    phoneNumberCredentials.VerificationCode = code;
                    break;
                }
                case AppleAuthCredentials appleCredentials:
                {
                    throw new NotImplementedException();
                }
            }
        }

        public string GetLogin()
        {
            switch (_registrationModel.Credentials)
            {
                case null:
                    Debug.LogError("Credentials not set");
                    return string.Empty;
                case EmailCredentials emailCredentials:
                    return emailCredentials.Email;
                case PhoneNumberCredentials phoneNumberCredentials:
                    return phoneNumberCredentials.PhoneNumber;
                    
                case AppleAuthCredentials appleCredentials:
                    throw new NotImplementedException();
            }

            return string.Empty;
        }

        public bool IsPhoneSignup()
        {
            return _registrationModel.Credentials is PhoneNumberCredentials;
        }

        public async Task<ValidationResult> ValidatePassword(string password, string nickname)
        {
            var isShort = password.Length < Constants.Onboarding.MIN_PASSWORD_LENGTH;
            if (isShort)
            {
                return new ValidationResult 
                { 
                    IsValid = false, 
                    ReasonPhrase = $"Use at least {Constants.Onboarding.MIN_PASSWORD_LENGTH} characters", 
                    RequirementFailed = new Dictionary<RequirementType, bool>()
                };
            }
            var validationResult  = await _bridge.ValidatePasswordStrength(password, nickname);

            var result = new ValidationResult
            {
                IsValid = validationResult.Model.Ok,
                ReasonPhrase = validationResult.Model.Error, 
                RequirementFailed = new Dictionary<RequirementType, bool>()
            };
            return result;
        }

        public async Task CreateTemporaryAccount()
        {
            SetupCountryAndLanguage();
            
            var model = new TemporaryAccountRequestModel
            {
                Country = _registrationModel.Country,
                DefaultLanguage = _registrationModel.DefaultLanguage
            };
            var result = await _bridge.RegisterTemporaryAccount(model, false);
            if (result.IsSuccess) return;
            
            Debug.LogError(result.ErrorMessage);
        }

        public Task<Result> SaveUserInfo()
        {
            SetupCountryAndLanguage();
            _localUserDataHolder.IsNewUser = true;
            var model = new UpdateUserModel
            {
                BirthDate = _registrationModel.BirthDate,
                DefaultLanguage = _registrationModel.DefaultLanguage,
                Country = _registrationModel.Country
            };
            return _bridge.UpdateUserInfo(model);
        }

        public async Task<string> GetNextUsernameSuggestion()
        {
            if (_usernamePool.Count > 0)
            {
                return _usernamePool.Pop();
            }

            var result = await _bridge.GetSuggestedNicknames(5);

            if (result.IsError)
            {
                Debug.LogError($"Failed to fetch suggested usernames, reason: {result.ErrorMessage}");
            }

            if (!result.IsSuccess)
            {
                return null;
            }

            foreach (var suggestion in result.Model)
            {
                _usernamePool.Push(suggestion);
            }

            return _usernamePool.Pop();
        }

        public async Task<List<string>> GetUsernameSuggestionList(int count)
        {
            var usernameList = new List<string>();

            while (usernameList.Count < count)
            {
                usernameList.Add(await GetNextUsernameSuggestion());
            }

            return usernameList;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SetupCountryAndLanguage()
        {
            _registrationModel.Country = _countryInfo.Iso2Code;
            _registrationModel.DefaultLanguage = "eng";

            #if UNITY_IOS && !UNITY_EDITOR
            var preferredLanguage = ISN_NSLocale.PreferredLanguage;
            _registrationModel.DefaultLanguage = preferredLanguage.Split('-')[0];
            #endif
        }

        private string PasswordValidationError(bool isShort, bool isSimple)
        {
            if (isSimple) return "This password is to simple";

            if (isShort) return $"Use at least {Constants.Onboarding.MIN_PASSWORD_LENGTH} characters";

            return string.Empty;
        }
        
        private async Task FetchUserCountryInfo()
        {
            if (_countryInfo != null)
            {
                return;
            }

            var locale = UM_Locale.GetCurrentLocale();

            var countryInfoResult = await _bridge.GetCountryInfoAsync(locale.CountryCode);

            _countryInfo = countryInfoResult.Model;
        }
    }
}