using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;

namespace Modules.Notifications.NotificationProviders
{
    internal sealed class VideoDeletedNotificationProvider : BaseNotificationProvider
    {
        public override NotificationType Type => NotificationType.VideoDeleted;
        public override NotificationItemModel GetNotificationItemModel(NotificationBase notification)
        {
            if (!IsValid(notification)) return null;
            
            var seasonLikesNotification = notification as VideoDeletedNotification;
            var itemModel = new NotificationVideoDeletedItemModel(seasonLikesNotification,  0);
            return itemModel;
        }

        protected override bool IsValid(NotificationBase notification)
        {
            return notification is VideoDeletedNotification videoDeletedNotification && videoDeletedNotification.Event != null;
        }
    }
}
