using AppStart;
using Modules.UserInitialization;
using UIManaging.Pages.Common.UserLoginManagement;
using UIManaging.Pages.Common.UsersManagement;
using Zenject;

namespace Installers
{
    internal static class UserServicesBinder
    {
        public static void BindUserServices(this DiContainer container)
        {
            container.Bind<UsersManager>().AsSingle();
            container.Bind<UserAccountManager>().AsSingle();
            container.Bind<LocalUserDataHolder>().AsSingle();
            container.Bind<UserProfileFetcher>().FromInstance(AppEntryContext.UserProfileFetcher);
            container.BindInterfacesAndSelfTo<UserInitializationService>().AsSingle();
        }
    }
}
