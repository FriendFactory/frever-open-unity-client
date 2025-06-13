using System;
using System.Linq;
using System.Threading;
using Bridge;
using Bridge.NotificationServer;
using Extensions;
using Modules.Notifications;
using Modules.Notifications.NotificationItemModels;
using Navigation.Core;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.PageHeader;
using UIManaging.Localization;
using UIManaging.Pages.Common.FollowersManagement;
using UIManaging.Pages.NotificationPage.NotificationSelection;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.NotificationPage
{
    internal sealed class NotificationPage : GenericPage<NotificationPageArgs>
    {
        [SerializeField] private PageHeaderView _pageHeaderView;
        [SerializeField] private NotificationListView _notificationListView;
        [SerializeField] private GameObject _noNotificationsText;
        [SerializeField] private NotificationTab[] _notificationTabs;
        
        [Inject] private PageManager _pageManager;
        [Inject] private INotificationHandler _notificationHandler;
        [Inject] private IBridge _bridge;
        [Inject] private NotificationsService _notificationsService;
        [Inject] private FollowersManager _followersManager;
        
        [Inject] private NotificationsLocalization _localization;

        private NotificationTabType _currentTab = NotificationTabType.Social;
        private CancellationTokenSource _cancellationToken;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public override PageId Id => PageId.NotificationPage;
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInit(PageManager pageManager)
        {
            _pageHeaderView.Init(new PageHeaderArgs(_localization.NotificationPageHeader, new ButtonArgs(string.Empty, _pageManager.MoveBack)));
        }

        protected override async void OnDisplayStart(NotificationPageArgs args)
        {
            base.OnDisplayStart(args);
            SetupTabs();
            await _followersManager.PrefetchDataForLocalUser();
            UpdateNotificationsList();
            _notificationHandler.NotificationAdded -= OnNotificationAdded;
            _notificationHandler.NotificationAdded += OnNotificationAdded;
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);
            _cancellationToken?.Cancel();
            _notificationListView.Cleanup();
            _notificationHandler.NotificationAdded -= OnNotificationAdded;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void UpdateNotificationsList()
        {
            var notifications = GetTargetNotifications();
            if (OpenPageArgs.ForceRefresh || !notifications.Any())
            {
                _cancellationToken = new CancellationTokenSource();
                _notificationsService.GetLatestNotifications(x => RefreshNotificationList(GetTargetNotifications()), _cancellationToken.Token);
            }
            else
            {
                RefreshNotificationList(notifications);
            }
        }

        private async void UpdateHasReadOnAllNotifications(NotificationItemModel[] notificationModels)
        {
            var unreadNotifications = notificationModels.Where(x => !x.HasRead).ToArray();
            var unreadNotificationsIds = unreadNotifications.Select(x => x.Id).ToArray();

            var markAsReadTask = await _bridge.MarkNotificationsAsRead(unreadNotificationsIds);

            if (markAsReadTask.IsSuccess)
            {
                foreach (var unreadNotification in unreadNotifications)
                {
                    unreadNotification.HasRead = true;
                }
            }
            else if (markAsReadTask.IsError)
            {
                Debug.LogError(
                    $"Failed to update mark as read {nameof(unreadNotifications)}. Reason: {markAsReadTask.ErrorMessage}");
            }
        }

        private async void RefreshNotificationList(NotificationItemModel[] models)
        {
            _noNotificationsText.SetActive(!models.Any());
            _notificationListView.SetActive(models.Any());
            if (models.Length == 0) return;

            if (models.Any(x => x.Type is NotificationType.NewFollower or NotificationType.FirstFollower))
            {
                await _followersManager.LocalUserFollower.RefreshListFromBackend();
            }
            
            var notificationListModel = new NotificationListModel(models, _localization);
            _notificationListView.Initialize(notificationListModel);

            foreach (var notificationTab in _notificationTabs)
            {
                notificationTab.SetCount(GetNonReadNotificationsCount(notificationTab.Type));
            }
            
            UpdateHasReadOnAllNotifications(models);
        }

        private void OnNotificationAdded()
        {
            if (IsDestroyed) return;
            var notifications = GetTargetNotifications();
            RefreshNotificationList(notifications);
        }

        private void SetupTabs()
        {
            foreach (var notificationTab in _notificationTabs)
            {
                notificationTab.Init(notificationTab.Type == _currentTab);
                notificationTab.Clicked += OnNotificationTabClicked;
            }
        }

        private void OnNotificationTabClicked(NotificationTabType tabType)
        {
            _currentTab = tabType;
            RefreshNotificationList(GetTargetNotifications());
        }
        
        private NotificationItemModel[] GetTargetNotifications()
        {
            return GetAllNotifications().Where(x => x.Type.IsTypeOf(_currentTab)).ToArray();
        }

        private NotificationItemModel[] GetAllNotifications()
        {
            return _notificationHandler.GetNotifications();
        }

        private int GetNonReadNotificationsCount(NotificationTabType tabType)
        {
            return GetAllNotifications().Count(notification => !notification.HasRead && notification.Type.IsTypeOf(tabType));
        }
    }
}