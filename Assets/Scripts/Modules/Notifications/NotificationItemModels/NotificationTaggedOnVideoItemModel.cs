using System;
using Bridge.NotificationServer;

namespace Modules.Notifications.NotificationItemModels
{
    public sealed class NotificationTaggedOnVideoItemModel : NotificationVideoItemModel
    {
        public NotificationTaggedOnVideoItemModel(long notificationId, long groupId, long videoId, long levelId, DateTime utcTimestamp, EventInfo ev) 
            : base(notificationId, groupId, videoId, levelId, utcTimestamp, ev)
        {
        }

        public override NotificationType Type => NotificationType.YouTaggedOnVideo;
    }
}
