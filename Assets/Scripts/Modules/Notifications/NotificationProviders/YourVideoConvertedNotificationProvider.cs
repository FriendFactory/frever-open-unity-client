using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;

namespace Modules.Notifications.NotificationProviders
{
    public sealed class YourVideoConvertedNotificationProvider : BaseNotificationProvider
    {
        public override NotificationType Type => NotificationType.YourVideoConverted;
        
        public override NotificationItemModel GetNotificationItemModel(NotificationBase notification)
        {
            if (IsValid(notification))
            {
                if (notification is YourVideoConversionCompletedNotification videoConvertedNotification)
                {
                    return new NotificationYourVideoConvertedItemModel(
                        videoConvertedNotification.Id,
                        videoConvertedNotification.Owner.Id,
                        videoConvertedNotification.ConvertedVideo.Id,
                        videoConvertedNotification.ConvertedVideo.LevelId ?? 0,
                        videoConvertedNotification.ConvertedVideo.ThumbnailUrl,
                        notification.Timestamp,
                        videoConvertedNotification.Event);
                }
            }

            return null;
        }

        protected override bool IsValid(NotificationBase notification)
        {
            return notification is YourVideoConversionCompletedNotification videoConvertedNotification
                   && videoConvertedNotification.ConvertedVideo != null;
        }
    }
}