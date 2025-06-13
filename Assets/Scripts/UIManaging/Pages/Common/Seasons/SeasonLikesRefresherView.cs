using System.Linq;
using Modules.Notifications;
using UIManaging.Pages.NotificationPage;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.Seasons
{
    public class SeasonLikesRefresherView: MonoBehaviour
    {
        [Inject] private ISeasonLikesNotificationHelper _likesHelper;
        [Inject] private INotificationHandler _notificationHandler;
        [Inject] private SnackBarHelper _snackBarHelper;

        private void OnEnable()
        {
            CheckLikesMilestones();

            _notificationHandler.UnreadNotificationAdded += CheckLikesMilestones;
        }

        private void OnDisable()
        {
            _notificationHandler.UnreadNotificationAdded -= CheckLikesMilestones;
        }

        public void CheckLikesMilestones()
        {
            var notifications = _likesHelper.GetNotifications();
            notifications = notifications.OrderBy(n => n.Likes).ToArray();

            notifications = notifications.OrderBy(n => n.Likes).ToArray();
            foreach (var notification in notifications)
            {
                _snackBarHelper.ShowSeasonLikesSnackBar(notification.Id, notification.QuestId, _likesHelper.GetNotificationText(notification.Likes));
            }
        }
    }
}