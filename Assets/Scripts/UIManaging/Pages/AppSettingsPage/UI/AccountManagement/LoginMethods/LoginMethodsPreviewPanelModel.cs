using System;
using System.Collections.Generic;
using Modules.AccountVerification.LoginMethods;

namespace UIManaging.Pages.AppSettingsPage.LoginMethods
{
    internal sealed class LoginMethodsPreviewPanelModel: IDisposable
    {
        private readonly LoginMethodsProvider _loginMethodsProvider;

        public event Action LoginMethodsUpdated;
        
        public LoginMethodsPreviewPanelModel(LoginMethodsProvider loginMethodsProvider)
        {
            _loginMethodsProvider = loginMethodsProvider;

            // maybe it is not the optimal to duplicate provider event in model
            _loginMethodsProvider.LoginMethodsUpdated += OnLoginMethodsUpdated;
        }
        
        public void Dispose()
        {
            _loginMethodsProvider.LoginMethodsUpdated -= OnLoginMethodsUpdated;
        }

        private void OnLoginMethodsUpdated() => LoginMethodsUpdated?.Invoke();

        public IEnumerable<ILoginMethodInfo> GetModels() => _loginMethodsProvider.LoginMethods;
    }
}