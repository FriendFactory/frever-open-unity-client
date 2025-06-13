using System;
using Bridge.NotificationServer;
using UIManaging.Common;

namespace Modules.Notifications.NotificationItemModels
{
    public abstract class NotificationItemModel : UserTimestampItemModel
    {
        public abstract NotificationType Type { get; }
        public long Id { get;}
        public bool HasRead { get; set; }

        protected NotificationItemModel(long notificationId, long groupId, DateTime utcTimestamp) : base(groupId,utcTimestamp)
        {
            Id = notificationId;
        }

        public abstract bool IsValid();
    }
}
