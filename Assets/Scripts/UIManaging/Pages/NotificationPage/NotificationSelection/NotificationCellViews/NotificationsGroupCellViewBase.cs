using Modules.Notifications.NotificationItemModels;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationCellViews
{
    public abstract class NotificationsGroupCellViewBase : UserBasedNotificationItemView<NotificationsGroupModel>
    {
        protected override void SetupDescriptionText()
        {
            var userName = UserProfile == null ? "Unknown" : UserProfile.NickName;
            _descriptionText.text = string.Format(Description, userName, ContextData.GroupedModels.Length);
            InvokeDescriptionSet();
        }
    }
}