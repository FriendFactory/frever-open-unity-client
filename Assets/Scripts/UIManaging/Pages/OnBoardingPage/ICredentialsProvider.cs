using Bridge.Authorization.Models;

namespace UIManaging.Pages.OnBoardingPage
{
    public interface ICredentialsProvider
    {
        ICredentials Credentials { get; }
        void ShowValidationError(string message);
    }
}