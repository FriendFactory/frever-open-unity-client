using System;
using Bridge.NotificationServer;

namespace Common.UserNotifications
{
    public interface INotificationEventSource
    {
        event Action<NotificationBase> NewNotificationReceived;
    }
}
