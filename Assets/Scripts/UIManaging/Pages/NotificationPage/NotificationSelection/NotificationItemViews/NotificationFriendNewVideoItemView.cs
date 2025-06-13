using Modules.Notifications.NotificationItemModels;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews
{
    public class NotificationFriendNewVideoItemView : NotificationVideoItemView<NotificationFriendNewVideoItemModel>
    {
        protected override string Description => _localization.FriendNewVideoFormat;
    }
}
