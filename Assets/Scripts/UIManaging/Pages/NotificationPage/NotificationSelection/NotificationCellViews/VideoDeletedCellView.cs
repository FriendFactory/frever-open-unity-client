using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemModels;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationCellViews
{
    public sealed class VideoDeletedCellView : NotificationCellView
    {
        public override NotificationType Type => NotificationType.VideoDeleted;
        public override void Initialize(NotificationItemModel model)
        {
            var view = GetComponent<NotificationVideoDeletedItemView>();
            var convertedModel = model as NotificationVideoDeletedItemModel;
            view.Initialize(convertedModel);
        }
    }
}
