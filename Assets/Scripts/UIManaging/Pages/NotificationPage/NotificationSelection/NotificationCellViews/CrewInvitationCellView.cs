using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationCellViews
{
    public class CrewInvitationCellView : UserBasedNotificationCellView<NotificationCrewInvitationItemView, NotificationCrewInvitationItemModel>
    {
        public override NotificationType Type => NotificationType.CrewInvitationReceived;
    }
}
