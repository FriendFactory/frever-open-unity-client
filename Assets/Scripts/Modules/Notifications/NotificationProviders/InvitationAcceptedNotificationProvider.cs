using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;
using UIManaging.Pages.Common.UsersManagement;

namespace Modules.Notifications.NotificationProviders
{
    internal sealed class InvitationAcceptedNotificationProvider: BaseNotificationProvider
    {
        private readonly LocalUserDataHolder _dataHolder;
        
        public override NotificationType Type => NotificationType.InvitationAccepted;

        public InvitationAcceptedNotificationProvider(LocalUserDataHolder dataHolder)
        {
            _dataHolder = dataHolder;
        }

        public override NotificationItemModel GetNotificationItemModel(NotificationBase notification)
        {
            if (!IsValid(notification)) return null;

            if (!(notification is InvitationAcceptedNotification targetNotification)) return null;

            return new NotificationInvitationAcceptedModel(targetNotification);
        }

        protected override bool IsValid(NotificationBase notification) =>
            !_dataHolder.IsStarCreator && notification is InvitationAcceptedNotification targetNotification &&
            targetNotification.AcceptedBy != null;
    }
}