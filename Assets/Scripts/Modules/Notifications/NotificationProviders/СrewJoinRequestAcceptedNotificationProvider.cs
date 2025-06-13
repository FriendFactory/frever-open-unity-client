using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;

namespace Modules.Notifications.NotificationProviders
{
    public class СrewJoinRequestAcceptedNotificationProvider : BaseNotificationProvider
    {
        public override NotificationType Type => NotificationType.CrewJoinRequestAccepted;
        public override NotificationItemModel GetNotificationItemModel(NotificationBase notification)
        {
            if (!IsValid(notification)) return null;
            
            var model = notification as CrewJoinRequestAcceptedNotification;

            return new NotificationCrewJoinRequestAcceptedItemModel(model.Id, 0, model.Timestamp, 
                                                                    "User", model.Crew.Name, model.Crew.Id);
        }

        protected override bool IsValid(NotificationBase notification)
        {
            var model = notification as CrewJoinRequestAcceptedNotification;
            return model?.Crew != null;
        }
    }
}