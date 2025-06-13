using System;
using Bridge.NotificationServer;

namespace Modules.Notifications.NotificationItemModels
{
    public class NotificationCrewInvitationItemModel : NotificationItemModel
    {
        public string UserNickname;
        public string CrewName;
        public long CrewId;

        public NotificationCrewInvitationItemModel(long notificationId, long groupId, DateTime utcTimestamp,
            string userNickname, string crewName, long crewId) : base(notificationId, groupId, utcTimestamp)
        {
            UserNickname = userNickname;
            CrewName = crewName;
            CrewId = crewId;
        }

        public override NotificationType Type => NotificationType.CrewInvitationReceived;
        public override bool IsValid() => true;
    }
}