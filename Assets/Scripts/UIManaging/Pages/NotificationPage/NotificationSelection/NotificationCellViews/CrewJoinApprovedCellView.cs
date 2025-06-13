using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationCellViews
{
    public class CrewJoinApprovedCellView : UserBasedNotificationCellView<NotificationCrewJoinApprovedItemView, NotificationCrewJoinRequestAcceptedItemModel>
    {
        public override NotificationType Type => NotificationType.CrewJoinRequestAccepted;
    }
}
