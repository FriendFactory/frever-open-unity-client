using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;

namespace Modules.Notifications.NotificationProviders
{
    internal sealed class NewLikeOnVideoNotificationProvider : BaseNotificationProvider
    {
        public override NotificationType Type => NotificationType.NewLikeOnVideo;
        
        public override NotificationItemModel GetNotificationItemModel(NotificationBase notification)
        {
            if (!IsValid(notification)) return null;
            
            var newLikeOnVideo = notification as NewLikeOnVideoNotification;
            
            var itemModel = new NotificationNewLikeOnVideoItemModel(
                newLikeOnVideo.Id, 
                newLikeOnVideo.LikedBy.Id, 
                newLikeOnVideo.LikedVideo.Id, 
                newLikeOnVideo.LikedVideo.LevelId ?? 0,
                notification.Timestamp,
                newLikeOnVideo.Event);
            
            return itemModel;
        }

        protected override bool IsValid(NotificationBase notification)
        {
            var newLikeOnVideo = notification as NewLikeOnVideoNotification;
            return newLikeOnVideo.LikedVideo != null && newLikeOnVideo.LikedBy != null;
        }
    }
}
