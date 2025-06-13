using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;

namespace Modules.Notifications.NotificationProviders
{
    internal sealed class RemixedYourVideoNotificationProvider : BaseNotificationProvider
    {
        public override NotificationType Type => NotificationType.YourVideoRemixed;

        public override NotificationItemModel GetNotificationItemModel(NotificationBase notification)
        {
            if (!IsValid(notification)) return null;
            
            var remixedYourVideo = notification as YourVideoRemixedNotification;
            var itemModel = new NotificationRemixedYourVideoItemModel(
                remixedYourVideo.Id,
                remixedYourVideo.RemixedBy.Id,
                remixedYourVideo.Remix.Id,
                remixedYourVideo.Remix.LevelId ?? 0,
                notification.Timestamp,
                remixedYourVideo.Event);

            return itemModel;
        }
        protected override bool IsValid(NotificationBase notification)
        {
            var remixedYourVideo = notification as YourVideoRemixedNotification;
            return remixedYourVideo.Remix != null && remixedYourVideo.RemixedBy != null;
        }
    }
}