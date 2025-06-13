using Modules.Notifications.NotificationItemModels;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemModels;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews
{
    public class NotificationTaggedOnVideoItemView : NotificationVideoItemView<NotificationTaggedOnVideoItemModel>
    {
        protected override string Description => _localization.TaggedOnVideoFormat;
    }
}
