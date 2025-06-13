using System;

namespace Modules.Notifications.NotificationItemModels
{
    public class NotificationGroup
    {
        public NotificationItemModel[] Models;
        public long FirstNotificationId;
        public long UserGroupId;
        public DateTime DateTime;
    }
}