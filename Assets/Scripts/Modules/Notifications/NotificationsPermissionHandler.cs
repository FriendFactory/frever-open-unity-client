using System;
using System.Collections.Generic;
using System.Globalization;
using Common.Permissions;
using Modules.Amplitude;
using JetBrains.Annotations;
using Navigation.Args;
using Navigation.Core;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using Zenject;

namespace Modules.Notifications
{
    [UsedImplicitly]
    public sealed class NotificationsPermissionHandler: IInitializable, IDisposable
    {
        private const string POPUP_SHOWN_DATE = "NotificationsPermissionPopupShownDate";

         private readonly IPermissionsHelper _permissionsHelper;
         private readonly PopupManager _popupManager;
         private readonly PageManager _pageManager;
         private string _prefKey;

         [Inject] private AmplitudeManager _amplitudeManager;

         //---------------------------------------------------------------------
         // Properties
         //---------------------------------------------------------------------

         private string PrefKey
         {
             get
             {
                 if (!string.IsNullOrEmpty(_prefKey)) return _prefKey;
                 _prefKey = $"{Application.identifier}.{POPUP_SHOWN_DATE}";
                 return _prefKey;
             }
         }

         private bool IsRequestedBefore => PlayerPrefs.HasKey(PrefKey);
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public NotificationsPermissionHandler(IPermissionsHelper permissionsHelper, PopupManager popupManager, PageManager pageManager)
        {
            _permissionsHelper = permissionsHelper;
            _popupManager = popupManager;
            _pageManager = pageManager;
        }

        public void Initialize()
        {
            _pageManager.PageDisplayed += OnPageDisplayed;

            _popupManager.PopupHidden += OnPopupHidden;
        }

        public void Dispose()
        {
            _pageManager.PageDisplayed -= OnPageDisplayed;

            _popupManager.PopupHidden -= OnPopupHidden;
        }
        
        //---------------------------------------------------------------------
        // Public 
        //---------------------------------------------------------------------
        
        public void RequestPermissionsIfNeeded(bool enforceRequestDelay = true)
        {
            var popupType = IsRequestedBefore ? PopupType.NotificationsPermissionSmall : PopupType.NotificationsPermissionLarge;
            
            RequestPermissionsIfNeeded(popupType, enforceRequestDelay);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void RequestPermissionsIfNeeded(PopupType popupType, bool enforceRequestDelay = true)
        {
            if (HasPermissionsAlready()) return;
            if (enforceRequestDelay && WerePermissionsRequestedRecently()) return;

            var contactPermissionPopupConfiguration = new DialogPopupConfiguration
            {
                PopupType = popupType,
                OnYes = AllowPermission, OnNo = NotNow
            };

            _popupManager.SetupPopup(contactPermissionPopupConfiguration);
            _popupManager.ShowPopup(contactPermissionPopupConfiguration.PopupType);

            void AllowPermission()
            {
                _popupManager.ClosePopupByType(PopupType.NotificationsPermissionLarge);
                RequestPermission();
            }

            void NotNow()
            {
                _popupManager.ClosePopupByType(PopupType.NotificationsPermissionLarge);
            }
        }

        private bool HasPermissionsAlready()
        {
            return _permissionsHelper.HasPermission(PermissionTarget.Notifications);
        }

        private bool WerePermissionsRequestedRecently()
        {
            // Check if permissions were requested in last 7 days
            if (PlayerPrefs.HasKey(PrefKey) &&
                DateTime.TryParse(PlayerPrefs.GetString(PrefKey), out var lastDisplayedDate) &&
                (DateTime.UtcNow - lastDisplayedDate).TotalDays < 7)
            {
                return true;
            }

            // Save the date when permissions were requested to player prefs
            PlayerPrefs.SetString(PrefKey, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));

            return false;
        }

        private void RequestPermission()
        {
            if (_permissionsHelper.GetPermissionState(PermissionTarget.Notifications).IsGranted()) return;
                
            if (_permissionsHelper.GetPermissionState(PermissionTarget.Notifications).IsDenied())
            {
                _permissionsHelper.OpenNativeAppPermissionMenu();
                return;
            }

            _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.NOTIFICATION_PROMPT_TAPPED);
            _permissionsHelper.RequestPermission(PermissionTarget.Notifications, Allowed, NotAllowed);
        }

        private void Allowed()
        {
            _amplitudeManager.LogEventWithEventProperties(
                AmplitudeEventConstants.EventNames.NOTIFICATION_PROMPT_CHOSEN,
                new Dictionary<string, object> { [AmplitudeEventConstants.EventProperties.NOTIFICATION_PROMPT_ACCEPTED] = true });
        }
        
        private void NotAllowed(string obj)
        {
            _amplitudeManager.LogEventWithEventProperties(
                AmplitudeEventConstants.EventNames.NOTIFICATION_PROMPT_CHOSEN,
                new Dictionary<string, object> { [AmplitudeEventConstants.EventProperties.NOTIFICATION_PROMPT_ACCEPTED] = false });
        }

        private void OnPopupHidden(PopupType popupType)
        {
            if (popupType != PopupType.PublishSuccess) return;

            _popupManager.PopupHidden -= OnPopupHidden;
            
            RequestPermissionsIfNeeded();
        }

        private void OnPageDisplayed(PageData pageData)
        {
            if (pageData.PageId is not (PageId.Feed or PageId.GamifiedFeed)) return;
            
            _pageManager.PageDisplayed -= OnPageDisplayed;
            
            RequestPermissionsIfNeeded();
        }
    }
}