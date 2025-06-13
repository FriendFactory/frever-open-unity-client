using System;
using Bridge.NotificationServer;

namespace Modules.Notifications.NotificationItemModels
{
    public class NotificationCommentOnVideoItemModel : NotificationVideoItemModel
    {
        public CommentInfo CommentInfo { get; }
        
        public NotificationCommentOnVideoItemModel(long notificationId, long groupId, long videoId, long levelId, DateTime utcTimestamp, CommentInfo commentInfo, EventInfo eventInfo) : base(notificationId, groupId, videoId, levelId, utcTimestamp, eventInfo)
        {
            CommentInfo = commentInfo;
        }

        public override NotificationType Type => NotificationType.NewCommentOnVideo;
    }
}
