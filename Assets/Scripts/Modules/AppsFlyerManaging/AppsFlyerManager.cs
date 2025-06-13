using System;
using System.Collections.Generic;
using AppsFlyerSDK;
using Modules.DeepLinking;
using Newtonsoft.Json;
using UnityEngine;
using Zenject;

namespace Modules.AppsFlyerManaging
{
    public class AppsFlyerManager : MonoBehaviour, IAppsFlyerConversionData, IAppsFlyerUserInvite
    {
        private const string ONE_LINK_ID = "zRzS";

        [SerializeField] private string devKey;
        [SerializeField] private string appID;
        [SerializeField] private string UWPAppID;
        [SerializeField] private string macOSAppID;
        [SerializeField] private bool isDebug;
        [SerializeField] private bool getConversionData;

        [Inject] private IInvitationLinkHandler _invitationLinkHandler;

        public string AppsFlyerId => AppsFlyer.getAppsFlyerId();

        private void OnEnable()
        {
            _invitationLinkHandler.InviteLinkRequested += OnInviteLinkRequested;
        }

        private void OnDisable()
        {
            _invitationLinkHandler.InviteLinkRequested -= OnInviteLinkRequested;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        public void StartSDK(bool disableAdvertisingTracking = false)
        {
            Debug.Log($"[{GetType().Name}] Starting AppsFlyer SDK # tracking: {disableAdvertisingTracking.ToString()}");
            
            // These fields are set from the editor so do not modify!
            //******************************//
        #if UNITY_WSA_10_0 && !UNITY_EDITOR
            AppsFlyer.initSDK(devKey, UWPAppID, getConversionData ? this : null);
        #elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
            AppsFlyer.initSDK(devKey, macOSAppID, getConversionData ? this : null);
        #else
            AppsFlyer.initSDK(devKey, appID, getConversionData ? this : null);
        #endif
            //******************************/
            
            AppsFlyer.setIsDebug(isDebug);
            AppsFlyer.anonymizeUser(true);
            AppsFlyer.setAppInviteOneLinkID(ONE_LINK_ID);
            
        #if UNITY_ANDROID && !UNITY_EDITOR
            AppsFlyer.setDisableAdvertisingIdentifiers(disableAdvertisingTracking);
        #endif
            
            AppsFlyer.OnDeepLinkReceived += OnDeepLinkReceived;
            
            AppsFlyer.startSDK();
        }

        public void StopSDK()
        {
            Debug.Log($"[{GetType().Name}] Stopping AppsFlyer SDK...");
            
            AppsFlyer.stopSDK(true);
        }

        //---------------------------------------------------------------------
        // AppsFlyer callbacks
        //---------------------------------------------------------------------

        public void onConversionDataSuccess(string conversionData)
        {
            AppsFlyer.AFLog("didReceiveConversionData", conversionData);
            var conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
        }

        public void onConversionDataFail(string error)
        {
            AppsFlyer.AFLog("didReceiveConversionDataWithError", error);
        }

        public void onAppOpenAttribution(string attributionData)
        {
            AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
            var attributionDataDictionary = AppsFlyer.CallbackStringToDictionary(attributionData);
        }

        public void onAppOpenAttributionFailure(string error)
        {
            AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
        }

        public void onInviteLinkGenerated(string link)
        {
            _invitationLinkHandler.HandleGeneratedInviteLink(link);
        }

        public void onInviteLinkGeneratedFailure(string error)
        {
            Debug.LogError($"[{GetType().Name}] Invite link generation failed: {error}");
        }

        public void onOpenStoreLinkGenerated(string link)
        {
            throw new NotImplementedException();
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnDeepLinkReceived(object sender, EventArgs args)
        {
            if (isDebug)
            {
                UnityEngine.Debug.Log($"[{GetType().Name}] {nameof(OnDeepLinkReceived)}:\n {JsonConvert.SerializeObject(args)}");
            }
            
            var deepLinkEventArgs = args as DeepLinkEventsArgs;

            if (deepLinkEventArgs?.deepLink == null)
            {
                if (isDebug)
                {
                    Debug.Log($"[{GetType().Name}] Deep link not found # {deepLinkEventArgs?.status}, {deepLinkEventArgs?.error}");
                }
                return;
            }
            
            // ignore universal links from our web.frever-api.com (shared video, profile, etc.)
            if (deepLinkEventArgs.deepLink.ContainsKey("host") && deepLinkEventArgs?.deepLink["host"] is string host &&
                host.Equals("web.frever-api.com")) return;

            if (deepLinkEventArgs?.status != DeepLinkStatus.FOUND) return;

        #if UNITY_ANDROID
            var deepLinkInfo = ConvertDeepLinkParamsToDeepLinkInfo(deepLinkEventArgs.deepLink);
        #else
            var deepLinkInfo = ConvertDeepLinkParamsToDeepLinkInfo((Dictionary<string, object>)deepLinkEventArgs.deepLink["click_event"]);
        #endif

            if (deepLinkInfo is null) return;

            // dependency is not resolved automatically on Android for some reasons
            _invitationLinkHandler ??= ProjectContext.Instance.Container.Resolve<IInvitationLinkHandler>();

            _invitationLinkHandler.Initialize(deepLinkInfo);
        }

        private InvitationLinkInfo ConvertDeepLinkParamsToDeepLinkInfo(Dictionary<string, object> deepLinkParams)
        {
            if (!deepLinkParams.TryGetValue(InvitationLinkHandler.GUID_KEY, out var guid))
            {
                UnityEngine.Debug.Log($"[{GetType().Name}] Guid key has not found - deep link conversion terminated");
                return null;
            }

            var userGroup = guid as string;

            return new InvitationLinkInfo(userGroup);
        }

        private void OnInviteLinkRequested(Dictionary<string, string> parameters)
        {
            if (isDebug)
            {
                UnityEngine.Debug.Log($"[{GetType().Name}] Generating invite link:\n {JsonConvert.SerializeObject(parameters)}");
            }
            
            AppsFlyer.generateUserInviteLink(parameters, this);
        }

        private void DebugLog(string message)
        {
            if (!isDebug) return;
            
            
        }
    }
}