using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;

namespace Modules.Notifications.NotificationProviders
{
    internal sealed class NewFollowerNotificationProvider : BaseNotificationProvider
    {
        public override NotificationType Type => NotificationType.NewFollower;
        public override NotificationItemModel GetNotificationItemModel(NotificationBase notification)
        {
            if (!IsValid(notification)) return null;
            
            var newFollowerNotification = notification as NewFollowerNotification;
            var itemModel = new NotificationNewFollowerItemModel(newFollowerNotification.Id, newFollowerNotification.FollowedBy.Id,
                                                                 notification.Timestamp, newFollowerNotification.FollowedBy.AreFriends, 
                                                                 newFollowerNotification.FollowedBy.Nickname);

            return itemModel;
        }

        protected override bool IsValid(NotificationBase notification)
        {
            var newFollowerNotification = notification as NewFollowerNotification;
            return newFollowerNotification.FollowedBy != null;
        }
    }
}
