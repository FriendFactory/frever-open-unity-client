using UIManaging.Pages.Common.FollowersManagement;
using UIManaging.Pages.Common.FollowersManagement.UserLists;
using Zenject;

namespace Installers
{
    public static class FollowersBinder
    {
        public static void BindFollowers(this DiContainer container)
        {
            container.Bind<LocalUserFollowedList>().AsSingle();
            container.Bind<LocalUserFollowerList>().AsSingle();
            container.BindFactoryCustomInterface<long, RemoteUserFollowedList, RemoteUserFollowedList.Factory, IFactory<long, RemoteUserFollowedList>>().AsSingle();
            container.BindFactoryCustomInterface<long, RemoteUserFollowerList, RemoteUserFollowerList.Factory, IFactory<long, RemoteUserFollowerList>>().AsSingle();
            container.Bind<FollowersManager>().AsSingle();
        }
    }
}