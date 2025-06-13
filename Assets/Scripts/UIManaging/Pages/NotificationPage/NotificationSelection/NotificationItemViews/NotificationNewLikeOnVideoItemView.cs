using Modules.Notifications.NotificationItemModels;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews
{
    public class NotificationNewLikeOnVideoItemView : NotificationVideoItemView<NotificationNewLikeOnVideoItemModel>
    {
        protected override string Description => _localization.NewLikeOnVideoFormat;
    }
}
