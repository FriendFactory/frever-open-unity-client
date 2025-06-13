using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationCellViews
{
    public class CrewJoinRequestedCellView : UserBasedNotificationCellView<NotificationCrewJoinRequestedItemView, NotificationCrewJoinRequestReceivedItemModel>
    {
        public override NotificationType Type => NotificationType.CrewJoinRequestReceived;
    }
}
