using System;
using Bridge.NotificationServer;

namespace Modules.Notifications.NotificationItemModels
{
    public sealed class NotificationNewMentionOnVideoItemModel : NotificationVideoItemModel
    {
        public override NotificationType Type => NotificationType.NewMentionOnVideo;

        public NotificationNewMentionOnVideoItemModel(long notificationId, long groupId, long videoId, long levelId, DateTime utcTimestamp, EventInfo ev) 
            : base(notificationId, groupId, videoId, levelId, utcTimestamp, ev)
        { }
    }
}