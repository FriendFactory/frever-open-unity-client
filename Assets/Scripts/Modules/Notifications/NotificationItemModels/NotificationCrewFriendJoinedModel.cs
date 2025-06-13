using System;
using Bridge.NotificationServer;

namespace Modules.Notifications.NotificationItemModels
{
    public class NotificationCrewFriendJoinedModel : NotificationItemModel
    {
        public readonly long CrewId;
        public readonly string UserNickname;
        
        public override NotificationType Type => NotificationType.FriendJoinedCrew;

        public NotificationCrewFriendJoinedModel(string userNickname, long notificationId, long crewId, long groupId, DateTime utcTimestamp) : base(notificationId, groupId, utcTimestamp)
        {
            CrewId = crewId;
            UserNickname = userNickname;
        }
        
        public override bool IsValid() => true;
    }
}