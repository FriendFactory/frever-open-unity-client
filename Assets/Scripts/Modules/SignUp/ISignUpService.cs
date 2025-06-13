using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bridge.Authorization.Models;
using Bridge.Results;

namespace Modules.SignUp
{
    public interface ISignUpService
    {
        string SelectedUserName { get; }

        Task Initialize();
        void CleanUp();
        void SetUserName(string name);
        void SetCredentials(ICredentials credentials);
        Task<ValidationResult> ValidateUsername();
        int SetBirthdayAndCalculateAge(int month, int year);
        Task<ValidationResult> ValidateEmail(EmailCredentials credentials);
        Task<ValidationResult> ValidatePhoneNumber(PhoneNumberCredentials credentials);
        Task RequestVerificationCode();
        void SetVerificationCode(string code);
        void RequestAppleIdCredentials(Action<AppleAuthCredentials> onSuccess, Action onFail);
        void RequestGooglePlayCredentials(Action<GoogleAuthCredentials> onSuccess, Action<string> onFail);
        string GetLogin();
        bool IsPhoneSignup();
        Task<ValidationResult> ValidatePassword(string password, string nickname);
        Task CreateTemporaryAccount();
        Task<Result> SaveUserInfo();
        Task<string> GetNextUsernameSuggestion();
        Task<List<string>> GetUsernameSuggestionList(int count);
    }
}