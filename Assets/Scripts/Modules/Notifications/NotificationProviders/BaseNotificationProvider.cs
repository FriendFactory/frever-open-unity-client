using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;

namespace Modules.Notifications.NotificationProviders
{
    public abstract class BaseNotificationProvider
    {
        public abstract NotificationType Type { get; }
        public abstract NotificationItemModel GetNotificationItemModel(NotificationBase notification);
        protected abstract bool IsValid(NotificationBase notification);
    }
}
