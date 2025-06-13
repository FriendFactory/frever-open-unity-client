using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;

namespace Modules.Notifications.NotificationProviders
{
    public class CommentOnVideoYouHaveCommentedOnNotificationProvider : BaseNotificationProvider
    {
        public override NotificationType Type => NotificationType.NewCommentOnVideoYouHaveCommented;
        
        public override NotificationItemModel GetNotificationItemModel(NotificationBase notification)
        {
            if (!IsValid(notification)) return null;
            
            var newCommentOnVideoYouHaveCommentedNotification = notification as NewCommentOnVideoYouHaveCommentedNotification;
            
            var itemModel = new NotificationCommentOnVideoYouHaveCommentedOnItemModel(
                newCommentOnVideoYouHaveCommentedNotification.Id, 
                newCommentOnVideoYouHaveCommentedNotification.CommentedBy.Id, 
                newCommentOnVideoYouHaveCommentedNotification.CommentedVideo.Id, 
                newCommentOnVideoYouHaveCommentedNotification.CommentedVideo.LevelId ?? 0,
                newCommentOnVideoYouHaveCommentedNotification.Timestamp,
                newCommentOnVideoYouHaveCommentedNotification.Comment,
                newCommentOnVideoYouHaveCommentedNotification.Event);

            return itemModel;
        }

        protected override bool IsValid(NotificationBase notification)
        {
            var newCommentOnVideoYouHaveCommentedNotification = notification as NewCommentOnVideoYouHaveCommentedNotification;
            return newCommentOnVideoYouHaveCommentedNotification.CommentedVideo != null && newCommentOnVideoYouHaveCommentedNotification.CommentedBy != null;
        }
    }
}
