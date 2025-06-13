using System;
using System.Collections.Generic;
using System.Linq;
using Bridge;
using JetBrains.Annotations;
using Modules.Notifications.NotificationItemModels;
using Navigation.Core;
using UIManaging.Pages.Common.UsersManagement.BlockedUsersManaging;
using UIManaging.SnackBarSystem;
using Zenject;

namespace Modules.Notifications
{
    [UsedImplicitly]
    internal sealed class NotificationHandler : INotificationHandler
    {
        [Inject] private IBlockedAccountsManager _blockedAccountsManager;
        [Inject] private SnackBarManager _snackbarManager;
        [Inject] private IBridge _bridge;
        [Inject] private PageManager _pageManager;
        
        private readonly List<NotificationItemModel> _notificationModels = new List<NotificationItemModel>();
        
        public event Action NotificationAdded;
        public event Action NotificationsCleared;
        public event Action UnreadNotificationAdded;

        public bool HasUnreadNotifications => GetNotifications().Any(x => !x.HasRead);
        public int UnreadNotificationsCount => GetNotifications().Count(x => !x.HasRead);

        public void AddNotifications(List<NotificationItemModel> models)
        {
            models.ForEach(m => _notificationModels.Add(m));
            NotificationAdded?.Invoke();
            
            if (models.Any(m => !m.HasRead)) UnreadNotificationAdded?.Invoke();
        }

        public void RemoveNotification(NotificationItemModel model)
        {
            _notificationModels.Remove(model);
        }

        public NotificationItemModel[] GetNotifications()
        {
            return _notificationModels.Where(x=>x.IsValid() && !_blockedAccountsManager.IsUserBlocked(x.GroupId)).ToArray();
        }

        public void ClearNotifications()
        {
            _notificationModels.Clear();
            NotificationsCleared?.Invoke();
        }
    }
}