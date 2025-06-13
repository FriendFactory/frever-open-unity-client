using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;

namespace Modules.Notifications.NotificationProviders
{
    internal sealed class VideoGotRatingNotificationProvider : BaseNotificationProvider
    {
        public override NotificationType Type => NotificationType.VideoRatingCompleted;

        public override NotificationItemModel GetNotificationItemModel(NotificationBase notification)
        {
            if (!IsValid(notification)) return null;
            if (notification is not RatedVideoNotification ratedVideoNotification) return null;

            var itemModel = new NotificationVideoGotRatingItemModel(
                ratedVideoNotification.Id,
                ratedVideoNotification.RatedVideo.Id,
                ratedVideoNotification.RatedVideo.LevelId ?? 0,
                notification.Timestamp,
                ratedVideoNotification.Event);

            return itemModel;
        }

        protected override bool IsValid(NotificationBase notification)
        {
            return (notification as RatedVideoNotification)?.RatedVideo != null;
        }
    }
}