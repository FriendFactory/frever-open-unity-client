using UIManaging.Pages.Common.UsersManagement.BlockedUsersManaging;
using Zenject;

namespace Installers
{
    internal static class BlockedAccountsServicesBinder
    {
        public static void BindBlockedAccountsServices(this DiContainer container)
        {
            container.BindInterfacesAndSelfTo<BlockedAccountsManager>().AsSingle();
        }
    }
}