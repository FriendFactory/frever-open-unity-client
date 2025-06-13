using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationCellViews
{
    public class CrewFriendJoinedCellView : UserBasedNotificationCellView<NotificationCrewFriendJoinedItemView, NotificationCrewFriendJoinedModel>
    {
        public override NotificationType Type => NotificationType.FriendJoinedCrew;
    }
}
