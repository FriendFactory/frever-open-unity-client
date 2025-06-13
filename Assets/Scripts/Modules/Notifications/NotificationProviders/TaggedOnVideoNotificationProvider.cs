using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;

namespace Modules.Notifications.NotificationProviders
{
    internal sealed class TaggedOnVideoNotificationProvider : BaseNotificationProvider
    {
        public override NotificationType Type => NotificationType.YouTaggedOnVideo;
        
        public override NotificationItemModel GetNotificationItemModel(NotificationBase notification)
        {
            if (!IsValid(notification)) return null;
            
            var taggedOnVideo = notification as YouTaggedOnVideoNotification;
            var itemModel = new NotificationTaggedOnVideoItemModel(
                taggedOnVideo.Id, 
                taggedOnVideo.TaggedBy.Id, 
                taggedOnVideo.TaggedOnVideo.Id, 
                taggedOnVideo.TaggedOnVideo.LevelId ?? 0,
                notification.Timestamp,
                taggedOnVideo.Event);
            
            return itemModel;
        }
        protected override bool IsValid(NotificationBase notification)
        {
            var taggedOnVideo = notification as YouTaggedOnVideoNotification;
            return taggedOnVideo.TaggedOnVideo != null && taggedOnVideo.TaggedBy != null;
        }
    }
}
