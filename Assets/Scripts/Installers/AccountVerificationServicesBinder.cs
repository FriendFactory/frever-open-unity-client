using Modules.AccountVerification;
using Modules.AccountVerification.LoginMethods;
using Modules.AccountVerification.Providers;
using UIManaging.Common.NotificationBadge;
using Zenject;

namespace Installers
{
    internal static class AccountVerificationServicesBinder
    {
        public static void BindAccountVerificationServices(this DiContainer container)
        {
            #if UNITY_IOS
            container.BindInterfacesAndSelfTo<AppleIdCredentialsHandler>().AsSingle();
            #elif UNITY_ANDROID
            container.BindInterfacesAndSelfTo<GoogleIdCredentialsHandler>().AsSingle();
            #endif
            container.BindInterfacesAndSelfTo<AccountVerificationService>().AsSingle();
            container.BindInterfacesAndSelfTo<LoginMethodsProvider>().AsSingle();
            container.BindInterfacesAndSelfTo<LoginMethodsNotificationBadgeDataProvider>().AsSingle();
        }
    }
}