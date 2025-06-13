using System;
using Bridge.NotificationServer;

namespace Modules.Notifications.NotificationItemModels
{
    public class NotificationCrewJoinRequestReceivedItemModel : NotificationItemModel
    {
        public readonly string UserNickname;
        public readonly string CrewName;
        public readonly long CrewId;
        public readonly long RequestId;

        public NotificationCrewJoinRequestReceivedItemModel(long notificationId, long requestId, long groupId, 
            DateTime utcTimestamp, string userNickname, string crewName, long crewId) 
            : base(notificationId, groupId, utcTimestamp)
        {
            RequestId = requestId;
            UserNickname = userNickname;
            CrewName = crewName;
            CrewId = crewId;
        }

        public override NotificationType Type => NotificationType.CrewJoinRequestReceived;
        public override bool IsValid() => true;
    }
}