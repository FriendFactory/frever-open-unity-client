using System;
using System.Linq;
using Bridge.AccountVerification.Models;
using JetBrains.Annotations;
using Modules.AccountVerification.LoginMethods;
using UIManaging.Pages.Common;
using Zenject;

namespace UIManaging.Common.NotificationBadge
{
    [UsedImplicitly]
    public sealed class LoginMethodsNotificationBadgeDataProvider: INotificationBadgeDataProvider, IInitializable, IDisposable
    {
        private readonly LoginMethodsProvider _loginMethodsProvider;
        
        public NotificationBadgeModel NotificationBadgeModel { get; private set; }

        public LoginMethodsNotificationBadgeDataProvider(LoginMethodsProvider loginMethodsProvider)
        {
            _loginMethodsProvider = loginMethodsProvider;
        }

        public void Initialize()
        {
            NotificationBadgeModel = new NotificationBadgeModel(LoginMethodsNotificationCount);

            _loginMethodsProvider.LoginMethodsUpdated += OnLoginMethodsUpdated;
        }

        public void Dispose()
        {
            _loginMethodsProvider.LoginMethodsUpdated -= OnLoginMethodsUpdated;
        }

        private void OnLoginMethodsUpdated() => NotificationBadgeModel.NotificationCount = LoginMethodsNotificationCount;

        private int LoginMethodsNotificationCount => _loginMethodsProvider.Initialized && !HasAnyLoginMethodSetup() ? 1 : 0;

        private bool HasAnyLoginMethodSetup()
        {
            return _loginMethodsProvider.LoginMethods.Count(x => x.IsLinked) > 0;
        }
    }
}