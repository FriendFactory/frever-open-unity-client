using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemModels;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationCellViews
{
    public class CommentOnVideoCellView : UserBasedNotificationCellView<NotificationCommentOnVideoItemView, NotificationCommentOnVideoItemModel>
    {
        public override NotificationType Type => NotificationType.NewCommentOnVideo;
    }
}
