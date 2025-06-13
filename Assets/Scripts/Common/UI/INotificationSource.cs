using System;

public interface INotificationSource
{
    event Action NotificationReceived;
    long? NotificationId { get; }
    long? QuestId { get; }
    bool ShouldRead { get; }
}
