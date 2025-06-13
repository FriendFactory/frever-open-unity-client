using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bridge;
using Bridge.Services.UserProfile;
using FriendFactory.AmplitudeExperiment;
using JetBrains.Annotations;
using SA.iOS.Foundation;
using UnityEngine;
using Zenject;

namespace Modules.Amplitude
{
    [UsedImplicitly]
    public sealed class AmplitudeManager : IInitializable
    {
        private const int AGE_LIMIT = 13;
        
        private const string INSTANCE_NAME = "disabledIpTracking";
        private const string ANALYTICS_KEY = "8686fbf1711a6cb2f21fa485c9f7faf2";
        private const string EXPERIMENTS_KEY = "client-ozkS1RqG67K8pPOlgGZDU7qSqp0ycvB5";
        
        public IDictionary<string, string> MlExperimentVariantsHeader
        {
            get
            {
                if (AmplitudeExperiment.Variants == null
                    || AmplitudeExperiment.Variants.Count == 0)
                {
                    LogEvent(AmplitudeEventConstants.EventNames.EXPERIMENT_FLAGS_EMPTY);
                }
                
                return AmplitudeExperiment.MLVariantsHeader;
            }
        }

        private static readonly Dictionary<string, bool> RESTRICTED_TRACKING_OPTIONS = new() {{"disableIPAddress", true}, {"disableCity", true}};
        private static readonly Dictionary<string, bool> UNRESTRICTED_TRACKING_OPTIONS = new() {{"disableIPAddress", true}};

        public bool IsInitialized { get; private set; }

        private bool UseRestrictedTrackingOptions { get; set; }
       
        public event Action<string> EventLogged;
        public event Action<string, IReadOnlyDictionary<string, object>> EventWithPropertiesLogged;

        private global::Amplitude _amplitudeInstance;

        [Inject] private IBridge _bridge;
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Initialize()
        {
            if (IsInitialized) return;

            var options = UseRestrictedTrackingOptions ? RESTRICTED_TRACKING_OPTIONS : UNRESTRICTED_TRACKING_OPTIONS;

            _amplitudeInstance = global::Amplitude.getInstance(INSTANCE_NAME);
            _amplitudeInstance.setTrackingOptions(options);
            _amplitudeInstance.init(ANALYTICS_KEY);
            ChangeCoppaControl(UseRestrictedTrackingOptions);
            
            IsInitialized = true;
            
            #if DISABLE_AMPLITUDE
            _amplitude.setOptOut(true);
            IsInitialized = false;
            #endif
        }

        public void ChangeTrackingOptions(int userAge)
        {
            var useRestrictedTrackingOptions = userAge < AGE_LIMIT;
            
            if (useRestrictedTrackingOptions == UseRestrictedTrackingOptions) return;
            
            if (IsInitialized)
            {
                CleanUp();
            }

            UseRestrictedTrackingOptions = useRestrictedTrackingOptions;
            
            Initialize();
        }

        public void ChangeCoppaControl(bool enable)
        {
            if (!CanSendEvents()) return;
            
            if (enable)
            {
                _amplitudeInstance.enableCoppaControl();
            }
            else
            {
                _amplitudeInstance.disableCoppaControl();
            }
        }
        
        public Task SetupAmplitude(int userAge)
        {
            ChangeTrackingOptions(userAge);
            SendCustomUserId();
            SendUserProperties();
            return SetupExperiment();
        }
        
        public void SendCustomUserId()
        {
            if (!CanSendEvents()) return;
            var userId = _bridge.Profile.Id.ToString();
            _amplitudeInstance.setUserId(userId);
        }

        public Task<bool> SetupExperiment()
        {
            return AmplitudeExperiment.Initialize(EXPERIMENTS_KEY, INSTANCE_NAME.ToLower());
        }
        
        public void SendUserProperties()
        {
            if (!CanSendEvents()) return;
            _amplitudeInstance.setUserProperty(AmplitudeEventConstants.UserProperties.USER_ID, _bridge.Profile.Id);
            _amplitudeInstance.setUserProperty(AmplitudeEventConstants.UserProperties.GROUP_ID, _bridge.Profile.GroupId);
            _amplitudeInstance.setUserProperty(AmplitudeEventConstants.UserProperties.BUILD_VERSION, Application.version);
            _amplitudeInstance.setUserProperty(AmplitudeEventConstants.UserProperties.BUILD_NUMBER, ISN_NSBundle.BuildInfo.BuildNumber);
        }

        public void SendUserCurrentLevelProperty(int level)
        {
            if (!CanSendEvents()) return;
            _amplitudeInstance.setUserProperty(AmplitudeEventConstants.UserProperties.USER_LEVEL, level);
        }
        
        public void SendUserCurrencyProperties(UserBalance userBalance)
        {
            if (!CanSendEvents()) return;
            _amplitudeInstance.setUserProperty(AmplitudeEventConstants.UserProperties.HARD_CURRENCY_AMOUNT, userBalance.HardCurrencyAmount);
            _amplitudeInstance.setUserProperty(AmplitudeEventConstants.UserProperties.SOFT_CURRENCY_AMOUNT, userBalance.SoftCurrencyAmount);
        }

        public void LogEvent(string eventName)
        {
            if (!CanSendEvents()) return;
            _amplitudeInstance.logEvent(eventName);
            
            EventLogged?.Invoke(eventName);
        }

        public void LogEventWithEventProperties(string eventName, IReadOnlyDictionary<string, object> eventProperties)
        {
            if (!CanSendEvents()) return;
            _amplitudeInstance.logEvent(eventName, eventProperties);
            
            EventWithPropertiesLogged?.Invoke(eventName, eventProperties);
        }

        public void UploadEventsNow()
        {
            if (!IsInitialized)
            {
                return;
            }
            
            _amplitudeInstance.uploadEvents();
        }

        //---------------------------------------------------------------------
        // Experiments
        //---------------------------------------------------------------------

        public static string GetVariantValue(string key)
        {
            return AmplitudeExperiment.GetVariantValue(key);
        }

        public string GetVariantPayload(string key)
        {
            return AmplitudeExperiment.GetPayloadValue(key);
        }
        
        public static IDictionary<string, string> GetVariantsList()
        {
            return AmplitudeExperiment.GetVariantsList();
        }

        public string GetDeviceId()
        {
            return _amplitudeInstance.getDeviceId();
        }
        
        //---------------------------------------------------------------------
        // Shopping Cart Experiment
        //---------------------------------------------------------------------

        public bool IsShoppingCartFeatureEnabled()
        {
            var variantValue = GetVariantValue("shopping-cart");
            return variantValue == "treatment";
        }
        
        //---------------------------------------------------------------------
        // Onboarding Quests Experiment
        //---------------------------------------------------------------------
        
        public bool IsOnboardingQuestsFeatureEnabled()
        {
            #if UNITY_EDITOR
                return true;
            #else
                var variantValue = GetVariantValue(AmplitudeExperimentConstants.ExperimentKeys.ONBOARDING_QUESTS);
                return variantValue == "treatment";
            #endif
        }
        
        //---------------------------------------------------------------------
        // For me Experiment
        //---------------------------------------------------------------------
       
        public static bool IsForMeFeatureEnabled()
        {
            var variantValue = GetVariantValue(AmplitudeExperimentConstants.ExperimentKeys.FOR_ME);
            // Hardcoded for task_mvp per Viktor's request
            //return  variantValue == "treatment";
            return true;
        }
        
        //---------------------------------------------------------------------
        // App review
        //---------------------------------------------------------------------

        public static bool GetInAppReviewEnabled()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    return GetVariantValue(AmplitudeExperimentConstants.ExperimentKeys.IN_APP_REVIEW_ANDROID) == "treatment";
                case RuntimePlatform.IPhonePlayer:
                    return GetVariantValue(AmplitudeExperimentConstants.ExperimentKeys.IN_APP_REVIEW_IOS) == "treatment";
                default:
                    return true;
            }
        }
        
        //---------------------------------------------------------------------
        // Feed follow trigger button
        //---------------------------------------------------------------------
        
        public static bool GetFeedFollowTriggerButtonEnabled()
        {
            return GetVariantValue(AmplitudeExperimentConstants.ExperimentKeys.FEED_FOLLOW_TRIGGER_BUTTON) == "treatment";
        }
        
        //---------------------------------------------------------------------
        // Gamified Feed UI
        //---------------------------------------------------------------------
        
        public static bool IsGamifiedFeedEnabled()
        {
            var variantValue = GetVariantValue(AmplitudeExperimentConstants.ExperimentKeys.GAMIFIED_FEED);

            return variantValue == "treatment";
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private bool CanSendEvents()
        {
            return IsInitialized && _bridge.Environment == FFEnvironment.Production;
        }

        private void CleanUp()
        {
            _amplitudeInstance.endSession();
            _amplitudeInstance = null;

            IsInitialized = false;
        }
        
        public void PrintExperimentHeaders()
        {
            AmplitudeExperiment.PrintExperimentHeaders();
        }
    }
}