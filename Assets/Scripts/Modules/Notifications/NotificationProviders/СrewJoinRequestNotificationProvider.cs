using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;

namespace Modules.Notifications.NotificationProviders
{
    public class СrewJoinRequestNotificationProvider : BaseNotificationProvider
    {
        public override NotificationType Type => NotificationType.CrewJoinRequestReceived;
        public override NotificationItemModel GetNotificationItemModel(NotificationBase notification)
        {
            if (!IsValid(notification)) return null;
            
            var model = notification as CrewJoinRequestReceivedNotification;

            return new NotificationCrewJoinRequestReceivedItemModel(model.Id, model.RequestId ?? 0,
                                                                    model.RequestedBy.Id, model.Timestamp,
                                                                    model.RequestedBy.Nickname,
                                                                    model.Crew.Name, model.Crew.Id);
        }

        protected override bool IsValid(NotificationBase notification)
        {
            var model = notification as CrewJoinRequestReceivedNotification;
            return model?.Crew != null && model.RequestedBy != null;
        }
    }
}