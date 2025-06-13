using System.Linq;
using System.Runtime.CompilerServices;
using Bridge;
using Bridge.NotificationServer;
using JetBrains.Annotations;
using Modules.Notifications;
using Modules.Notifications.NotificationItemModels;
using UIManaging.Pages.NotificationPage;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemModels;
using UnityEngine;
using Zenject;

[assembly: InternalsVisibleTo("Installers")]
namespace UIManaging.Pages.Common.Seasons
{
    [UsedImplicitly]
    internal sealed class SeasonLikesNotificationHelper: ISeasonLikesNotificationHelper
    {
        [Inject] private IBridge _bridge;
        [Inject] private INotificationHandler _notificationHandler;
        
        public NotificationSeasonLikesItemModel[] GetNotifications()
        {
            return _notificationHandler.GetNotifications()
                                       .Where(notification =>
                                                  !notification.HasRead &&
                                                  notification.Type ==
                                                  NotificationType.SeasonQuestAccomplished)
                                       .Cast<NotificationSeasonLikesItemModel>()
                                       .ToArray();
        }

        public string GetNotificationText(int likes)
        {
            var likesWord = likes > 1 ? "likes" : "like";
            
            return $"You got {likes} {likesWord}!";
        }

        public async void MarkNotificationAsDisplayed(long notificationId)
        {
            var targetNotification = _notificationHandler.GetNotifications()
                                                         .FirstOrDefault(notification => notification.Id == notificationId);

            if (targetNotification != null)
            {
                
                if (targetNotification.HasRead)
                {
                    return;
                }
                
                targetNotification.HasRead = true;
            }
            
            var result = await _bridge.MarkNotificationsAsRead(new[] {notificationId});

            if (result.IsError)
            {
                Debug.LogError($"Failed to mark like milestone notification {notificationId} as read, reason: {result.ErrorMessage}");
            }

            if (!result.IsSuccess)
            {
                
                targetNotification = _notificationHandler.GetNotifications()
                                                             .FirstOrDefault(notification => notification.Id == notificationId);

                if (targetNotification != null)
                {
                    targetNotification.HasRead = false;
                }
            }
        }
    }
}