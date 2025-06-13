using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemModels;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationCellViews
{
    public class NonCharacterTagOnVideoCellView : UserBasedNotificationCellView<NotificationNonCharacterTagOnVideoItemView, NotificationNonCharacterTagOnVideoItemModel>
    {
        public override NotificationType Type => NotificationType.NonCharacterTagOnVideo;
    }
}
