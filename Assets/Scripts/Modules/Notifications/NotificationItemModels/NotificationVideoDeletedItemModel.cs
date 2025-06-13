using Bridge.NotificationServer;

namespace Modules.Notifications.NotificationItemModels
{
    public sealed class NotificationVideoDeletedItemModel : NotificationItemModel
    {
        public VideoDeletedNotification Notification { get; }
        
        public NotificationVideoDeletedItemModel(VideoDeletedNotification notification, long groupId) : base(notification.Id, groupId, notification.Timestamp)
        {
            Notification = notification;
        }

        public override NotificationType Type => NotificationType.VideoDeleted;
        public override bool IsValid()
        {
            return true;
        }
    }
}
