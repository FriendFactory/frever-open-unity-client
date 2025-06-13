using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemModels;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationCellViews
{
    public class NewFriendVideoCellView : UserBasedNotificationCellView<NotificationFriendNewVideoItemView, NotificationFriendNewVideoItemModel>
    {
        public override NotificationType Type => NotificationType.NewFriendVideo;
    }
}
