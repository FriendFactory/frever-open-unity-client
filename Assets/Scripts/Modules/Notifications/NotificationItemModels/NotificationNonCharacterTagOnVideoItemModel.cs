using System;
using Bridge.NotificationServer;

namespace Modules.Notifications.NotificationItemModels
{
    public sealed class NotificationNonCharacterTagOnVideoItemModel : NotificationVideoItemModel
    {
        public NotificationNonCharacterTagOnVideoItemModel(long notificationId, long groupId, long videoId, long levelId, DateTime utcTimestamp, EventInfo ev) 
            : base(notificationId, groupId, videoId, levelId, utcTimestamp, ev)
        {
        }

        public override NotificationType Type => NotificationType.NonCharacterTagOnVideo;
    }
}
