using System;
using Bridge.NotificationServer;

namespace Modules.Notifications.NotificationItemModels
{
    public class NotificationCrewJoinRequestAcceptedItemModel : NotificationItemModel
    {
        public readonly long CrewId;
        public readonly string CrewName;
        public readonly string UserNickname;

        public NotificationCrewJoinRequestAcceptedItemModel(long notificationId, long groupId, DateTime utcTimestamp,
            string memberNickname, string crewName, long crewId) 
            : base(notificationId, groupId, utcTimestamp)
        {
            CrewName = crewName;
            CrewId = crewId;
            UserNickname = memberNickname;
        }

        public override NotificationType Type => NotificationType.CrewJoinRequestAccepted;
        public override bool IsValid() => true;
    }
}