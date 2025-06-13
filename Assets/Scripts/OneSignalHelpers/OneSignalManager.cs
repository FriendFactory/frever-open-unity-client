using System;
using System.Collections.Generic;
using Bridge;
using Modules.Amplitude;
using Navigation.Args;
using Navigation.Core;
using OneSignalSDK;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.NotificationPage;
using UnityEngine;
using Zenject;
using LogLevel = OneSignalSDK.LogLevel;

namespace OneSignalHelpers
{
    public class OneSignalManager
    {
        private const string WEB_NOTIFICATION_KEY = "WEB";
        private const string NOTIFICATION_TYPE_KEY = "Type";
        private const string CREW_ID_KEY = "Id";
        private const string CHAT_ID_KEY = "Id";
        
        private static AmplitudeManager _amplitudeManager;

        [Inject] private PageManager _pageManager;
        [Inject] private LocalUserDataHolder _userData;
        [Inject] private IBridge _bridge;

        private bool _applicationWasPaused;
        private bool _isInFocusNotification;
        private bool _switchPageRequested;

        private Notification _openedNotification;
        
        private readonly Dictionary<FFEnvironment, string> _appIds = new Dictionary<FFEnvironment, string>()
        {
            {FFEnvironment.ProductionUSA, "0af04ca2-0c20-44f3-a28c-feb4b6dde315"},
            {FFEnvironment.Production, "0af04ca2-0c20-44f3-a28c-feb4b6dde315"},
            {FFEnvironment.Develop, "71811127-8e0c-4bed-aa9b-0d3e635f8797"},
            {FFEnvironment.Stage, "5d52a770-570b-42ce-b7df-5b963f104fda"},
            {FFEnvironment.Test, "4cbcb9ae-f894-4339-9961-5c06752a06e2"}
        };

        private readonly HashSet<PageId> _rediractablePages = new HashSet<PageId>()
        {
            PageId.Feed,
            PageId.HomePage,
            PageId.HomePageSimple
        };

        public void Initialize(FFEnvironment environment, AmplitudeManager amplitudeManager)
        {
            _amplitudeManager = amplitudeManager;
            _pageManager.ApplicationFocus += OnApplicationFocus;
            // Uncomment this method to enable OneSignal Debugging log output 
            OneSignal.Default.LogLevel = LogLevel.Verbose;
            OneSignal.Default.AlertLevel = LogLevel.None;

            // Replace 'YOUR_ONESIGNAL_APP_ID' with your OneSignal App ID.
            OneSignal.Default.NotificationOpened += HandleNotificationOpened;
            OneSignal.Default.NotificationWillShow += ReceivedNotification;
            OneSignal.Default.Initialize(_appIds[environment]);
            OneSignal.Default.SetLaunchURLsInApp(false);
        }

        public void SetExternalUserId(string userId)
        {
            OneSignal.Default.SetExternalUserId(userId);
        }

        public void RemoveExternalUser()
        {
            OneSignal.Default.RemoveExternalUserId();
        }

        private void HandleNotificationOpened(NotificationOpenedResult result)
        {
            _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.APP_OPEN_FROM_NOTIFICATION);

            _openedNotification = result.notification;
            
            if (Application.platform == RuntimePlatform.Android && _isInFocusNotification)
            {
                _switchPageRequested = true;
                return;
            }
            
            if (_openedNotification.additionalData != null 
             && _openedNotification.additionalData.ContainsKey(WEB_NOTIFICATION_KEY)) return;

            if (_applicationWasPaused || _isInFocusNotification)
            {
                RedirectToTargetPage();
                return;
            }

            _pageManager.PageDisplayed += OnPageDisplayed;
        }
        
        private Notification ReceivedNotification(Notification notification)
        {
            _isInFocusNotification = true;
            return notification;
        }

        private void OnPageDisplayed(PageData pageData)
        {
            if (!_rediractablePages.Contains(pageData.PageId)) return;

            _pageManager.PageDisplayed -= OnPageDisplayed;
            
            _applicationWasPaused = false;
            RedirectToTargetPage();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            _applicationWasPaused = !hasFocus;
            
            if (Application.platform != RuntimePlatform.Android || !_switchPageRequested) return;
            
            _switchPageRequested = false;
            RedirectToTargetPage();
        }

        private void RedirectToTargetPage()
        {
            var notificationType = PushNotificationType.Default;

            try
            {
                _openedNotification.additionalData.TryGetValue(NOTIFICATION_TYPE_KEY, out var type);
                if (type != null)
                {
                    notificationType = (PushNotificationType)(long)type;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            switch (notificationType)
            {
                case PushNotificationType.CrewMessageNotification:
                    MoveToCrewPage();
                    break;
                case PushNotificationType.ChatMessageNotification:
                    MoveToChatPage();
                    break;
                case PushNotificationType.CrewNotification:
                case PushNotificationType.Default:
                default:
                    MoveToNotificationPage();
                    break;
            }
        }

        private void MoveToNotificationPage()
        {
            _isInFocusNotification = false;
            
            if (_pageManager.IsCurrentPage(PageId.NotificationPage)) return;

            if (Application.platform == RuntimePlatform.Android && _applicationWasPaused)
            {
                _switchPageRequested = true;
            }
            else
            {
                _pageManager.MoveNext(PageId.NotificationPage, new NotificationPageArgs {ForceRefresh = true});
            }
        }
        
        private void MoveToCrewPage()
        {
            var crewId = (long)_openedNotification.additionalData[CREW_ID_KEY];
            
            if (_userData.UserProfile.CrewProfile?.Id != crewId)
            {
                MoveToNotificationPage();
                return;
            }
            
            _isInFocusNotification = false;
            
            if (_pageManager.IsCurrentPage(PageId.CrewPage)) return;

            if (Application.platform == RuntimePlatform.Android && _applicationWasPaused)
            {
                _switchPageRequested = true;
                return;
            }

            _pageManager.MoveNext(PageId.CrewPage, new CrewPageArgs());
        }
        
        private async void MoveToChatPage()
        {
            var chatId = (long)_openedNotification.additionalData[CHAT_ID_KEY];
            
            _isInFocusNotification = false;

            if (Application.platform == RuntimePlatform.Android && _applicationWasPaused)
            {
                _switchPageRequested = true;
                return;
            }

            var chatRequest = await _bridge.GetChatById(chatId);

            if (chatRequest.IsError)
            {
                MoveToNotificationPage();
                return;
            }
                
            _pageManager.MoveNext(new ChatPageArgs(chatRequest.Model));
        }
    }
}