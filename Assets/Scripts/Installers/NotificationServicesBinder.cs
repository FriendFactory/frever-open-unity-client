using Modules.Notifications;
using OneSignalHelpers;
using UIManaging.Pages.NotificationPage;
using Zenject;

namespace Installers
{
    internal static class NotificationServicesBinder
    {
        public static void BindNotificationServices(this DiContainer container)
        {
            container.Bind<OneSignalManager>().AsSingle();
            container.BindInterfacesAndSelfTo<NotificationHandler>().AsSingle();
            container.BindInterfacesAndSelfTo<NotificationsService>().AsSingle();
            container.BindInterfacesAndSelfTo<StyleBattleNotificationHelper>().AsSingle();
        }
    }
}
