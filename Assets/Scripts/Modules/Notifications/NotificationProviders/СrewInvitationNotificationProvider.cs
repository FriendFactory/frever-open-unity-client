using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;

namespace Modules.Notifications.NotificationProviders
{
    public class СrewInvitationNotificationProvider : BaseNotificationProvider
    {
        public override NotificationType Type => NotificationType.CrewInvitationReceived;
        public override NotificationItemModel GetNotificationItemModel(NotificationBase notification)
        {
            if (!IsValid(notification)) return null;
            
            var model = notification as CrewInvitationReceivedNotification;

            return new NotificationCrewInvitationItemModel(model.Id, model.InvitedBy.Id,
                                                             model.Timestamp, model.InvitedBy.Nickname, 
                                                             model.Crew.Name, model.Crew.Id);
        }

        protected override bool IsValid(NotificationBase notification)
        {
            var model = notification as CrewInvitationReceivedNotification;
            return model?.InvitedBy != null && model.Crew != null;
        }
    }
}