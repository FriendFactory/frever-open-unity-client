using Modules.AccountVerification.LoginMethods;

namespace UIManaging.Pages.AppSettingsPage.LoginMethods
{
    internal sealed class LoginMethodPreviewModel
    {
        public ILoginMethodInfo LoginMethodInfo { get; }
        public string Label { get; }

        public LoginMethodPreviewModel(ILoginMethodInfo loginMethodInfo, string label)
        {
            LoginMethodInfo = loginMethodInfo;
            Label = label;
        }
    }
}