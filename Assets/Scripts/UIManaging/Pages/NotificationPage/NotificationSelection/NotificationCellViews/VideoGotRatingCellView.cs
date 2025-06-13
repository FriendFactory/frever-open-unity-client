using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationCellViews
{
    public class VideoGotRatingCellView : UserBasedNotificationCellView<NotificationVideoGotRatingItemView, NotificationVideoGotRatingItemModel>
    {
        public override NotificationType Type => NotificationType.VideoRatingCompleted;
    }
}