using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;

namespace Modules.Notifications.NotificationProviders
{
    internal sealed class FriendNewVideoNotificationProvider : BaseNotificationProvider
    {
        public override NotificationType Type => NotificationType.NewFriendVideo;
        
        public override NotificationItemModel GetNotificationItemModel(NotificationBase notification)
        {
            if (!IsValid(notification)) return null;
            
            var newFriendVideo = notification as NewFriendVideoNotification;
            var itemModel = new NotificationFriendNewVideoItemModel(
                newFriendVideo.Id, 
                newFriendVideo.PostedBy.Id, 
                newFriendVideo.NewVideo.Id, 
                newFriendVideo.NewVideo.LevelId ?? 0,
                notification.Timestamp,
                newFriendVideo.Event);
            
            return itemModel;
        }
        protected override bool IsValid(NotificationBase notification)
        {
            var newFriendVideo = notification as NewFriendVideoNotification;
            return newFriendVideo.NewVideo != null && newFriendVideo.PostedBy != null;
        }
    }
}
