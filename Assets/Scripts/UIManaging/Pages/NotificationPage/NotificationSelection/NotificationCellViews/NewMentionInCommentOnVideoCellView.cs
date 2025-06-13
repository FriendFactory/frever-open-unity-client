using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemModels;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationCellViews
{
    public sealed class NewMentionInCommentOnVideoCellView : UserBasedNotificationCellView<NotificationNewMentionInCommentOnVideoView, NotificationNewMentionInCommentOnVideoItemModel>
    {
        public override NotificationType Type => NotificationType.NewMentionInCommentOnVideo;
    }
}