using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationCellViews
{
    internal sealed class InvitationAcceptedCellView: NotificationCellView
    {
        public override NotificationType Type => NotificationType.InvitationAccepted;
        
        public override void Initialize(NotificationItemModel model)
        {
            var view = GetComponent<NotificationInvitationAcceptedItemView>();
            var convertedModel = model as NotificationInvitationAcceptedModel;
            view.Initialize(convertedModel);
        }
    }
}