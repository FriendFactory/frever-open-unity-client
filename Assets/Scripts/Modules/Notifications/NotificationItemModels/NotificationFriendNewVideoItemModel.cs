using System;
using Bridge.NotificationServer;

namespace Modules.Notifications.NotificationItemModels
{
    public sealed class NotificationFriendNewVideoItemModel : NotificationVideoItemModel
    {
        public NotificationFriendNewVideoItemModel(long notificationId, long groupId, long videoId, long levelId, DateTime utcTimestamp, EventInfo eventInfo) 
            : base(notificationId, groupId, videoId, levelId, utcTimestamp, eventInfo)
        {
        }


        public override NotificationType Type => NotificationType.NewFriendVideo;
    }
}
