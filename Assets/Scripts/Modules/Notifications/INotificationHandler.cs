using System;
using System.Collections.Generic;
using Modules.Notifications.NotificationItemModels;

namespace Modules.Notifications
{
    public interface INotificationHandler
    {
        event Action NotificationAdded;
        event Action NotificationsCleared;
        event Action UnreadNotificationAdded;
        
        bool HasUnreadNotifications { get; }
        int UnreadNotificationsCount { get; }
        
        void AddNotifications(List<NotificationItemModel> models);
        void RemoveNotification(NotificationItemModel model);
        void ClearNotifications();
        NotificationItemModel[] GetNotifications();
    }
}
