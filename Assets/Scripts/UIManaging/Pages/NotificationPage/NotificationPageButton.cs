using JetBrains.Annotations;
using Modules.Notifications;
using Navigation.Core;
using UIManaging.Core;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.NotificationPage
{
    internal sealed class NotificationPageButton : ButtonBase
    {
        [SerializeField] private GameObject _onStateIcon;
        [SerializeField] private GameObject _offStateIcon;
        private INotificationHandler _notificationHandler;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        private void Construct(INotificationHandler notificationHandler)
        {
            _notificationHandler = notificationHandler;
            _offStateIcon.SetActive(true);
            _onStateIcon.SetActive(false);
            _notificationHandler.UnreadNotificationAdded += SetUnreadNotificationState;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void OnEnable()
        {
            base.OnEnable();
            if (_notificationHandler.HasUnreadNotifications)
            {
                SetUnreadNotificationState();
            }
        }

        private void OnDestroy()
        {
            if (_notificationHandler == null) return;
            _notificationHandler.UnreadNotificationAdded -= SetUnreadNotificationState;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
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

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SetUnreadNotificationState()
        {
            if (_onStateIcon == null || _offStateIcon == null) return;
            _onStateIcon.SetActive(true);
            _offStateIcon.SetActive(false);
        }

        private void OnAllNotificationsRead()
        {
            if (_onStateIcon == null || _offStateIcon == null) return;
            _onStateIcon.SetActive(false);
            _offStateIcon.SetActive(true);
        }
    }
}
