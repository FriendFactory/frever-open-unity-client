using System;
using Bridge.NotificationServer;

namespace Modules.Notifications.NotificationItemModels
{
    public sealed class NotificationStyleBattleResultItemModel : NotificationItemModel
    {

        public NotificationStyleBattleResultItemModel(long notificationId, long groupId, DateTime utcTimestamp) 
            : base(notificationId, groupId, utcTimestamp)
        {
        }

        public override NotificationType Type => NotificationType.BattleResultCompleted;
        
        public override bool IsValid()
        {
            return GroupId != 0;
        }
    }
}