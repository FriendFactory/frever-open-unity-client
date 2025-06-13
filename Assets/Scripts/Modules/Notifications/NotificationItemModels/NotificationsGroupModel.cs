using System;
using System.Linq;
using Bridge.NotificationServer;
using UIManaging.Common;

namespace Modules.Notifications.NotificationItemModels
{
    public sealed class NotificationsGroupModel: NotificationItemModel
    {
        public NotificationItemModel[] GroupedModels;
        public override NotificationType Type => GroupedModels.First().Type;
        public bool Expanded;
        
        public NotificationsGroupModel(long notificationId, long groupId, DateTime utcTimestamp) : base(notificationId, groupId, utcTimestamp)
        {
        }
        
        public override bool IsValid()
        {
            return true;
        }
    }
}