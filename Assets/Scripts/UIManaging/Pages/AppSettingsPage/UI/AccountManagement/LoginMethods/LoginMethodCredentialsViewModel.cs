using Modules.AccountVerification;

namespace UIManaging.Pages.AppSettingsPage.LoginMethods
{
    public class LoginMethodCredentialsViewModel
    {
        public IVerificationMethod VerificationMethod { get; }

        public LoginMethodCredentialsViewModel(IVerificationMethod verificationMethod)
        {
            VerificationMethod = verificationMethod;
        }
    }
}