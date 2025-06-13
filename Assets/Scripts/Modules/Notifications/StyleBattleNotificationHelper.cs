using System.Linq;
using Bridge;
using Bridge.NotificationServer;
using Modules.Notifications.NotificationItemModels;
using Navigation.Args;
using Navigation.Core;
using UIManaging.SnackBarSystem;
using UIManaging.SnackBarSystem.Configurations;
using UnityEngine;
using Zenject;

namespace Modules.Notifications
{
    internal sealed class StyleBattleNotificationHelper
    {
        [Inject] private IBridge _bridge;
        [Inject] private INotificationHandler _notificationHandler;
        [Inject] private SnackBarManager _snackBarManager;
        [Inject] private PageManager _pageManager;

        public void HandleStyleBattleNotification()
        {
            var validNotifications = GetNotifications();
            if (validNotifications.Length != 0)
            {
                ShowStyleBattleSnackbar(validNotifications[0]);
            }
            
            foreach (var notificationModel in validNotifications)
            {
                MarkNotificationAsDisplayed(notificationModel.Id);
            }
        }

        private NotificationStyleBattleResultItemModel[] GetNotifications()
        {
            return _notificationHandler.GetNotifications()
                                       .Where(n =>
                                                  !n.HasRead &&
                                                  n.Type == NotificationType.BattleResultCompleted)
                                       .Cast<NotificationStyleBattleResultItemModel>()
                                       .ToArray();
        }
        
        private void ShowStyleBattleSnackbar(NotificationItemModel notificationItemModel)
        {
            var configuration = new StyleBattleResultCompletedSnackbarConfiguration(OnViewButtonClick);
            _snackBarManager.Show(configuration);

            async void OnViewButtonClick()
            {
                notificationItemModel.HasRead = true;
                var result = await _bridge.GetTaskFullInfoAsync(notificationItemModel.GroupId);
                if (result.IsError)
                {
                    Debug.LogError(result.ErrorMessage);
                    return;
                }

                var taskInfo = result.Model;
                var args = new VotingResultPageArgs(taskInfo.Id, taskInfo.Name);
                _pageManager.MoveNext(args);
            }
        }

        private async void MarkNotificationAsDisplayed(long notificationId)
        {
            var targetNotification =
                _notificationHandler.GetNotifications().FirstOrDefault(n => n.Id == notificationId);

            if (targetNotification != null && targetNotification.HasRead)
            {
                targetNotification.HasRead = true;
            }

            var result = await _bridge.MarkNotificationsAsRead(new[] { notificationId });
            if (result.IsError)
            {
                Debug.LogError($"Failed to mark style battle notification {notificationId} as read\nreason: {result.ErrorMessage}");
            }

            if (!result.IsSuccess && targetNotification != null)
            {
                targetNotification.HasRead = false;
            }
        }
    }
}