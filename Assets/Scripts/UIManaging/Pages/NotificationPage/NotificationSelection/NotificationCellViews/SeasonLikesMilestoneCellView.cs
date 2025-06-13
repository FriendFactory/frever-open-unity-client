using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationCellViews
{
    public sealed class SeasonLikesMilestoneCellView : NotificationCellView
    {
        public override NotificationType Type => NotificationType.SeasonQuestAccomplished;
        public override void Initialize(NotificationItemModel model)
        {
            var view = GetComponent<NotificationSeasonLikesItemView>();
            var convertedModel = model as NotificationSeasonLikesItemModel;
            view.Initialize(convertedModel);
        }
    }
}
