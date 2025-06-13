using System;
using System.Collections.Generic;
using Bridge;
using Bridge.Authorization.Models;
using Common.ExceptionCatcher;
using Modules.Amplitude;
using JetBrains.Annotations;
using Sentry;
using Sentry.Unity;
#if !UNITY_EDITOR && UNITY_IOS 
using Sentry.Unity.iOS;
#endif
using UnityEngine;

namespace Modules.SentryManaging
{
    [UsedImplicitly]
    public sealed class SentryManager
    {
        private const int EXCEPTION_WAIT_PERIOD = 5;
        
        private readonly AmplitudeManager _amplitudeManager;
        private readonly IgnoredExceptionsData _ignoredExceptions;
        private readonly IBridge _bridge;

        private readonly Dictionary<string, DateTime> _cachedExceptions = new Dictionary<string, DateTime>();
        private readonly List<FFEnvironment> _environments = new List<FFEnvironment>()
        {
            FFEnvironment.Production,
            FFEnvironment.Stage
        };

        private SentryUnityOptions _options;

        public SentryManager(IBridge bridge, AmplitudeManager amplitudeManager, IgnoredExceptionsData ignoredExceptions)
        {
            _bridge = bridge;
            _amplitudeManager = amplitudeManager;
            _ignoredExceptions = ignoredExceptions;
        }

        public void Reinitialize()
        {
            var sentryUnityInfo = new SentryUnityInfo();
            var options = ScriptableSentryUnityOptions.LoadSentryUnityOptions(sentryUnityInfo);
            if (options == null)
            {
                Debug.LogError($"[{GetType().Name}] Sentry initialization has aborted - Unity options loading failed");
                return;
            }

            _options = options;

            if (!options.ShouldInitializeSdk()) return;

            Debug.Log($"[{GetType().Name}] Sentry will be reinitialized - overriding Sentry options...");

            options.SetBeforeSend(OnBeforeSend);
            options.AddIntegration(new AmplitudeEventsIntegration(_amplitudeManager));

            SentryUnity.Init(options);
        }

        public void UpdateUserProfileScope(UserProfile profile, bool isModeratorOrEmployee)
        {
            Debug.Log(
                $"[{GetType().Name}] User profile scope has updated: {profile.Id}, isEmployee: {isModeratorOrEmployee}"); 
            
            SentrySdk.ConfigureScope(scope =>
            {
                scope.User.Id = profile.Id.ToString();
                scope.User.Other[nameof(UserProfile.GroupId)] = $"{profile.GroupId.ToString()}";

                if (_options == null)
                {
                    Debug.LogWarning($"[{GetType().Name}] Failed to override native user scope");
                    return;
                }
                
                #if !UNITY_EDITOR && UNITY_IOS 
                var iosScopedObserver = new NativeScopeObserver("iOS", _options);
                iosScopedObserver.SetUser(scope.User);
                #endif
            });
        }

        private bool ShouldBeIgnored(string message) =>
            !string.IsNullOrEmpty(message) && 
            _ignoredExceptions.ShouldBeIgnored(message) && !_ignoredExceptions.ShouldSendToAnalytics(message);

        private SentryEvent OnBeforeSend(SentryEvent sentryEvent)
        {
            var environment = _bridge.Environment;

            if (!_environments.Contains(environment)) return null;

            var stackTrace = sentryEvent.Exception?.StackTrace;
            
            if (IsExceptionRecentlySent(stackTrace)) return null;

            sentryEvent.Environment = environment.ToString();
            // removing potentially sensitive user data
            sentryEvent.Contexts.Device.Name = null;

            var containsIgnoredMessage = ShouldBeIgnored(sentryEvent.Message?.Message);
            var containsIgnoredException = ShouldBeIgnored(sentryEvent.Exception?.Message) || ShouldBeIgnored(stackTrace);

            var skipSendEvent = containsIgnoredMessage || containsIgnoredException;
            if (skipSendEvent) return null;

            CacheException(stackTrace);
            return sentryEvent;
        }
        
        private bool IsExceptionRecentlySent(string stackTrace)
        {
            return !string.IsNullOrEmpty(stackTrace) &&
                   (_cachedExceptions.ContainsKey(stackTrace) && DateTime.Now.Subtract(_cachedExceptions[stackTrace]).Seconds < EXCEPTION_WAIT_PERIOD);
        }
        
        private void CacheException(string stackTrace)
        {
            if (string.IsNullOrEmpty(stackTrace)) return;
            
            _cachedExceptions[stackTrace] =  DateTime.Now;
        }
    }
}