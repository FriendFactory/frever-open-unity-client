using System;
using System.Collections.Generic;
using Bridge.Authorization.Results;
using Bridge.Results;
using UIManaging.Localization;
using Zenject;

namespace UIManaging.Pages.Common.ErrorsManagement
{
    public class ErrorsManager
    {
        [Inject] private OnBoardingLocalization _onBoardingLocalization;

        public string GetLoginErrorMessageByType(LoginErrorType errorType)
        {
            switch (errorType)
            {
                case LoginErrorType.Other: return _onBoardingLocalization.VerificationCodeValidationOverrideMessage;
                case LoginErrorType.WrongEmail: return "Email is wrong";
                case LoginErrorType.WrongPassword: return _onBoardingLocalization.UsernameContentErrorMessage;
                default: return errorType.ToString();
            }
        }
        
        public string GetLRegistrationErrorMessageByType(RegistrationErrorType errorType)
        {
            switch (errorType)
            {
                case RegistrationErrorType.Other: return _onBoardingLocalization.VerificationCodeValidationOverrideMessage;
                case RegistrationErrorType.UserAlreadyExists: return "User with these credentials is already registered";
                default: return errorType.ToString();
            }
        }
    }
}