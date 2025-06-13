using Modules.Notifications.NotificationItemModels;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationCellViews
{
    public abstract class UserBasedNotificationCellView<T, N> : NotificationCellView where N : NotificationItemModel
        where T : UserBasedNotificationItemView<N>
    {
        protected N Model { get; private set; }
        
        public override void Initialize(NotificationItemModel model)
        {
            var view = GetComponent<T>();
            Model = model as N;
            view.Initialize(Model);

            view.DescriptionSet -= RefreshLayout;
            view.DescriptionSet += RefreshLayout;
        }
    }
}