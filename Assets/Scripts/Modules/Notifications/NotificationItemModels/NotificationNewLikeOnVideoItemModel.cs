using System;
using Bridge.NotificationServer;

namespace Modules.Notifications.NotificationItemModels
{
    public sealed class NotificationNewLikeOnVideoItemModel : NotificationVideoItemModel
    {
        public override NotificationType Type => NotificationType.NewLikeOnVideo;

        public NotificationNewLikeOnVideoItemModel(long notificationId, long groupId, long videoId, long levelId, DateTime utcTimestamp, EventInfo eventInfo) :
            base(notificationId, groupId, videoId, levelId, utcTimestamp, eventInfo)
        {
        }
    }
}
