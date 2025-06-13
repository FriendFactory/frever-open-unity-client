using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationCellViews
{
    public class NewFollowerCellView : UserBasedNotificationCellView<NotificationNewFollowerItemView, NotificationNewFollowerItemModel>
    {
        public override NotificationType Type => NotificationType.NewFollower;
    }
}
