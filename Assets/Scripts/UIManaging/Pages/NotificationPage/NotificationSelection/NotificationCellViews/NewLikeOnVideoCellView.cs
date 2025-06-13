using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationCellViews
{
    public sealed class NewLikeOnVideoCellView : UserBasedNotificationCellView<NotificationNewLikeOnVideoItemView, NotificationNewLikeOnVideoItemModel>
    {
        public override NotificationType Type => NotificationType.NewLikeOnVideo; 
    }
}
