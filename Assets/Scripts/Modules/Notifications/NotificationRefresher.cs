using System.Threading;
using UnityEngine;
using Zenject;

namespace Modules.Notifications
{
    /// <summary>
    /// Use it in scenes/pages where you care about getting notifications
    /// It runs auto-refreshing when the gameobject is enabled and stops when it's disabled
    /// </summary>
    internal sealed class NotificationRefresher: MonoBehaviour
    {
        [Inject] private NotificationsService _notificationsService;
        [Inject] private readonly StyleBattleNotificationHelper _battleNotificationHelper;
        
        private CancellationTokenSource _notificationsCancellationToken;
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                StartRefreshingNotifications();
            }
            else
            {
                StopRefreshingNotifications();
            }
        }

        private void OnEnable()
        {
            StartRefreshingNotifications();
        }

        private void OnDisable()
        {
            StopRefreshingNotifications();
        }

        private void StartRefreshingNotifications()
        {
            _notificationsCancellationToken = new CancellationTokenSource();
            _notificationsService.GetLatestNotifications(token: _notificationsCancellationToken.Token);
            _notificationsService.Start();
            
            _battleNotificationHelper.HandleStyleBattleNotification();
        }

        private void StopRefreshingNotifications()
        {
            _notificationsCancellationToken?.Cancel();
            _notificationsService.StopRefreshing();
        }
    }
}