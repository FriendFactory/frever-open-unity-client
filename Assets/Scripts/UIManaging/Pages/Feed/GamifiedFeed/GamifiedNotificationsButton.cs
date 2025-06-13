using BrunoMikoski.AnimationSequencer;
using JetBrains.Annotations;
using Modules.Notifications;
using Navigation.Core;
using UIManaging.Core;
using UIManaging.Pages.NotificationPage;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Feed.GamifiedFeed
{
    public class GamifiedNotificationsButton: ButtonBase
    {
        [SerializeField] private GameObject _notificationBadgeIcon;
        
        private INotificationHandler _notificationHandler;

        [Inject] 
        [UsedImplicitly]
        private void Construct(INotificationHandler notificationHandler)
        {
            _notificationHandler = notificationHandler;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            SetUnreadState(_notificationHandler.HasUnreadNotifications);

            _notificationHandler.UnreadNotificationAdded += OnUnreadAdded;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            _notificationHandler.UnreadNotificationAdded -= OnUnreadAdded;
        }

        protected override void OnClick()
        {
            var args = new NotificationPageArgs();
            var transitionArgs = new PageTransitionArgs
            {
                SaveCurrentPageToHistory = true,
                TransitionFinishedCallback = OnAllNotificationsRead
            };

            Manager.MoveNext(PageId.NotificationPage, args, transitionArgs);
        }

        private void OnUnreadAdded() => SetUnreadState(true);
        private void SetUnreadState(bool isOn) => _notificationBadgeIcon.SetActive(isOn);
        private void OnAllNotificationsRead() => SetUnreadState(false);
    }
}