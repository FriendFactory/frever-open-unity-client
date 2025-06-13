using System;
using Bridge.NotificationServer;

namespace Modules.Notifications.NotificationItemModels
{
    public class NotificationNewFollowerItemModel : NotificationItemModel
    {
        public readonly bool AreFriends;
        public readonly string UserNickname;
        
        public NotificationNewFollowerItemModel(long notificationId, long groupId, DateTime utcTimestamp, bool areFriends, string userNickname) : base(notificationId, groupId, utcTimestamp)
        {
            AreFriends = areFriends;
            UserNickname = userNickname;
        }

        public override NotificationType Type => NotificationType.NewFollower;
        public override bool IsValid()
        {
            return GroupId != 0;
        }
    }
}
