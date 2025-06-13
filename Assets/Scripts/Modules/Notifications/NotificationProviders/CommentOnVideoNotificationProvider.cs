using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;

namespace Modules.Notifications.NotificationProviders
{
    internal sealed class CommentOnVideoNotificationProvider : BaseNotificationProvider
    {
        public override NotificationType Type => NotificationType.NewCommentOnVideo;
        public override NotificationItemModel GetNotificationItemModel(NotificationBase notification)
        {
            if (!IsValid(notification)) return null;
            
            var newCommentOnVideoNotification = notification as NewCommentOnVideoNotification;
            
            var itemModel = new NotificationCommentOnVideoItemModel(
                newCommentOnVideoNotification.Id, 
                newCommentOnVideoNotification.CommentedBy.Id, 
                newCommentOnVideoNotification.CommentedVideo.Id, 
                newCommentOnVideoNotification.CommentedVideo.LevelId ?? 0,
                notification.Timestamp,
                newCommentOnVideoNotification.Comment,
                newCommentOnVideoNotification.Event);
            
            return itemModel;
        }

        protected override bool IsValid(NotificationBase notification)
        {
            var newCommentOnVideoNotification = notification as NewCommentOnVideoNotification;
            return newCommentOnVideoNotification?.CommentedVideo != null && newCommentOnVideoNotification.CommentedBy != null;
        }
    }
}
