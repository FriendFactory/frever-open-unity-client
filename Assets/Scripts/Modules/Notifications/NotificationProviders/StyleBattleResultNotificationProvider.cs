using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;

namespace Modules.Notifications.NotificationProviders
{
    public class StyleBattleResultNotificationProvider : BaseNotificationProvider
    {
        public override NotificationType Type => NotificationType.BattleResultCompleted;
        public override NotificationItemModel GetNotificationItemModel(NotificationBase notification)
        {
            if (!IsValid(notification)) return null;
            
            var notificationModel = notification as StyleBattleResultCompletedNotification;
            var itemModel = new NotificationStyleBattleResultItemModel(notificationModel.Id, 
                                                                       notificationModel.TaskId,
                                                                       notificationModel.Timestamp);
            
            return itemModel;
        }

        protected override bool IsValid(NotificationBase notification)
        {
            var styleBattleNotificationModel = notification as StyleBattleResultCompletedNotification;
            return styleBattleNotificationModel.TaskId != 0;
        }
    }
}