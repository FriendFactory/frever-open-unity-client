using Modules.Notifications.NotificationItemModels;

namespace UIManaging.Pages.Common.Seasons
{
    public interface ISeasonLikesNotificationHelper
    {
        NotificationSeasonLikesItemModel[] GetNotifications();
        string GetNotificationText(int likes);
        void MarkNotificationAsDisplayed(long notificationId);
    }
}