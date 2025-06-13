using UIManaging.Common.NotificationBadge;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.NotificationBadge
{
    internal sealed class UnverifiedAccountNotificationBadge: MonoBehaviour
    {
        [SerializeField] private NotificationBadge _notificationBadge;

        [Inject] private LoginMethodsNotificationBadgeDataProvider _notificationBadgeDataProvider;

        private void OnEnable()
        {
            _notificationBadge.Initialize(_notificationBadgeDataProvider.NotificationBadgeModel);
        }
        
        private void OnDisable()
        {
            _notificationBadge.CleanUp();
        }
    }
}