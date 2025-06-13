using System;
using Bridge.NotificationServer;
using CommentInfo = Bridge.NotificationServer.CommentInfo;

namespace Modules.Notifications.NotificationItemModels
{
    public sealed class NotificationCommentOnVideoYouHaveCommentedOnItemModel : NotificationCommentOnVideoItemModel
    {
        public override NotificationType Type => NotificationType.NewCommentOnVideoYouHaveCommented;

        public NotificationCommentOnVideoYouHaveCommentedOnItemModel(long notificationId, long groupId, long videoId, long levelId, DateTime utcTimestamp, CommentInfo commentInfo, EventInfo eventInfo) 
            : base(notificationId, groupId, videoId, levelId, utcTimestamp, commentInfo, eventInfo)
        {
        }
    }
}