using System;
using Bridge.NotificationServer;

namespace Modules.Notifications.NotificationItemModels
{
    public class NotificationRemixedYourVideoItemModel : NotificationVideoItemModel
    {
        public NotificationRemixedYourVideoItemModel(long notificationId, long groupId, long videoId, long levelId, DateTime utcTimestamp, EventInfo ev) 
            : base(notificationId, groupId, videoId, levelId, utcTimestamp, ev)
        {
        }

        public override NotificationType Type => NotificationType.YourVideoRemixed;
    }
}
