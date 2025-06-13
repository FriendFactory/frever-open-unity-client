using System;
using AppsFlyerSDK;
using Bridge;
using JetBrains.Annotations;
using Modules.DeepLinking;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using Zenject;

namespace Modules.AppsFlyerManaging
{
    public interface IAppsFlyerService
    {
        bool UserOptedOut { get; }

        void ToggleTracking(bool? optedOut);
        void StartAppsFlyer();
    }

    [UsedImplicitly]
    public class AppsFlyerService : IAppsFlyerService
    {
        private readonly LocalUserDataHolder _localUser;
        private readonly AppsFlyerManager _appsFlyerManager;
        private readonly IBridge _bridge;
        private readonly InvitationCodeModel _invitationCodeModel;

        private bool? _optedOutDuringSignUp;
        private bool _userOptedOut;

        [Inject]
        public AppsFlyerService(AppsFlyerManager appsFlyerManager, LocalUserDataHolder localUser, IBridge bridge,
            InvitationCodeModel invitationCodeModel)
        {
            _localUser = localUser;
            _appsFlyerManager = appsFlyerManager;
            _bridge = bridge;
            _invitationCodeModel = invitationCodeModel;
        }

        public bool UserOptedOut
        {
            get
            {
                if (_optedOutDuringSignUp != null)
                {
                    return _optedOutDuringSignUp.Value;
                }
                
                return _userOptedOut;
            }
        }

        public void ToggleTracking(bool? optedOut)
        {
            if (!_bridge.IsLoggedIn)
            {
                _optedOutDuringSignUp = optedOut;
                return;
            }

            if (optedOut is null)
            {
                Debug.LogError($"[{GetType().Name}] Opted out can't be set to null when user is logged in");
                return;
            }
            
            if (!optedOut.Value)
            {
                StartAppsFlyer();
                return;
            }

            DisableAdvertisingTracking();
        }

        #pragma warning disable CS1998
        public async void StartAppsFlyer()
            #pragma warning restore CS1998
        {
            try
            {
                _appsFlyerManager.StartSDK(UserOptedOut);
                
                if (UserOptedOut) return;

            #if UNITY_ANDROID
                Debug.Log($"[{GetType().Name}] Adding tracking id: {_appsFlyerManager.AppsFlyerId}");
            
                var addResult = await _bridge.AddTrackingId(_appsFlyerManager.AppsFlyerId);
                if (addResult.IsError) Debug.LogError(addResult.ErrorMessage);
            #endif

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public async void OnLoggedIn()
        {
            var userProfileResult = await _bridge.GetCurrentUserInfo();
            
            if (userProfileResult.IsError) Debug.Log(userProfileResult.ErrorMessage);
            if (userProfileResult.IsSuccess) _userOptedOut = !userProfileResult.Profile.AdvertisingTrackingEnabled;

            StartAppsFlyer();
            TrySendReturnedAfterADayEvent();
            InitializeInvitationCodeModel();
        }

        public void OnLoggedOut()
        {
            _optedOutDuringSignUp = false;
            _userOptedOut = false;
            _appsFlyerManager.StopSDK();
        }

        private async void DisableAdvertisingTracking()
        {
            try
            {
                await _bridge.RemoveAllTrackingIds();
                _userOptedOut = true;
                
                // restart SDK with tracking disabled
                _appsFlyerManager.StopSDK();
                _appsFlyerManager.StartSDK(true);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void TrySendReturnedAfterADayEvent()
        {
            var timeSpan = DateTime.UtcNow - _localUser.RegistrationDate;
            if (timeSpan.TotalDays >= 1 && timeSpan.TotalDays < 2)
            {
                AppsFlyer.sendEvent(AppsFlyerConstants.USER_RETURNED_DAY_1, null);
            }
        }

        private async void InitializeInvitationCodeModel()
        {
            try
            {
                await _invitationCodeModel.InitializeAsync();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}