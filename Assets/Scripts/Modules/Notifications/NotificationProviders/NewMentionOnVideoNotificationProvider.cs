using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;

namespace Modules.Notifications.NotificationProviders
{
    internal sealed class NewMentionOnVideoNotificationProvider : BaseNotificationProvider
    {
        public override NotificationType Type => NotificationType.NewMentionOnVideo;
        public override NotificationItemModel GetNotificationItemModel(NotificationBase notification)
        {
            if (!IsValid(notification)) return null;
            
            var newMentionOnVideoNotification = notification as NewMentionOnVideoNotification;
            
            var itemModel = new NotificationNewMentionOnVideoItemModel(
                newMentionOnVideoNotification.Id, 
                newMentionOnVideoNotification.MentionedBy.Id, 
                newMentionOnVideoNotification.MentionedVideo.Id, 
                newMentionOnVideoNotification.MentionedVideo.LevelId ?? 0,
                notification.Timestamp,
                newMentionOnVideoNotification.Event);
            
            return itemModel;
        }

        protected override bool IsValid(NotificationBase notification)
        {
            var newCommentOnVideoNotification = notification as NewMentionOnVideoNotification;
            return newCommentOnVideoNotification?.MentionedVideo != null && newCommentOnVideoNotification.MentionedBy != null;
        }
    }
}
