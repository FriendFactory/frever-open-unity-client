using System;
using Bridge.NotificationServer;

namespace Modules.Notifications.NotificationItemModels
{
    public sealed class NotificationNewMentionInCommentOnVideoItemModel : NotificationVideoItemModel
    {
        public CommentInfo CommentInfo { get; }
        
        public override NotificationType Type => NotificationType.NewMentionInCommentOnVideo;
        
        public NotificationNewMentionInCommentOnVideoItemModel(long notificationId, long groupId, long videoId, long levelId, DateTime utcTimestamp, EventInfo thumbnailEventInfo, CommentInfo commentInfo) : base(notificationId, groupId, videoId, levelId, utcTimestamp, thumbnailEventInfo)
        {
            CommentInfo = commentInfo;
        }
    }
}