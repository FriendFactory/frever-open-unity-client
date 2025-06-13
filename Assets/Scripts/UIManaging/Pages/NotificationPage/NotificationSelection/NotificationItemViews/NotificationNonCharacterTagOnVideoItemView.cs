using Modules.Notifications.NotificationItemModels;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews
{
    public class NotificationNonCharacterTagOnVideoItemView : NotificationVideoItemView<NotificationNonCharacterTagOnVideoItemModel>
    {
        protected override string Description => _localization.NonCharacterTagOnVideoFormat;
    }
}
