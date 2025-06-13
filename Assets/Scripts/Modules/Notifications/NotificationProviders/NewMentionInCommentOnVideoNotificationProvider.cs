using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;

namespace Modules.Notifications.NotificationProviders
{
    internal sealed class NewMentionInCommentOnVideoNotificationProvider : BaseNotificationProvider
    {
        public override NotificationType Type => NotificationType.NewMentionInCommentOnVideo;
        public override NotificationItemModel GetNotificationItemModel(NotificationBase notification)
        {
            if (!IsValid(notification)) return null;
            
            var notificationModel = notification as NewMentionInCommentOnVideo;
            
            var itemModel = new NotificationNewMentionInCommentOnVideoItemModel(
                notificationModel.Id, 
                notificationModel.CommentedBy.Id, 
                notificationModel.CommentedVideo.Id, 
                notificationModel.CommentedVideo.LevelId ?? 0,
                notification.Timestamp,
                notificationModel.Event,
                notificationModel.Comment);
            
            return itemModel;
        }

        protected override bool IsValid(NotificationBase notification)
        {
            var newCommentOnVideoNotification = notification as NewMentionInCommentOnVideo;
            return newCommentOnVideoNotification?.CommentedVideo != null && newCommentOnVideoNotification.CommentedBy != null;
        }
    }
}