using Modules.Notifications.NotificationItemModels;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemModels;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews
{
    public class NotificationRemixedYourVideoItemView : NotificationVideoItemView<NotificationRemixedYourVideoItemModel>
    {
        protected override string Description => _localization.YourVideoRemixedFormat;
    }
}
