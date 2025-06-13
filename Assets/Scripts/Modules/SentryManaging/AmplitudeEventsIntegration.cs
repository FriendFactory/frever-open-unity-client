using System;
using System.Collections.Generic;
using Modules.Amplitude;
using Sentry;
using Sentry.Integrations;

namespace Modules.SentryManaging
{
    internal sealed class AmplitudeEventsIntegration : ISdkIntegration
    {
        private readonly AmplitudeManager _amplitudeManager; 
        private readonly List<string> _ignoredEvents = new List<string> { "error_message" };
        
        public AmplitudeEventsIntegration(AmplitudeManager amplitudeManager)
        {
            _amplitudeManager = amplitudeManager ?? throw new ArgumentNullException($"{nameof(amplitudeManager)}");
        }

        public void Register(IHub hub, SentryOptions options)
        {
            _amplitudeManager.EventLogged += OnEventLogged;
            _amplitudeManager.EventWithPropertiesLogged += OnEventWithPropertiesLogged;
        }

        void OnEventLogged(string eventName)
        {
            if (_ignoredEvents.Contains(eventName)) return;
            
            SentrySdk.AddBreadcrumb($"Amplitude event logged: {eventName}", "amplitude.event");
        }

        private void OnEventWithPropertiesLogged(string eventName, IReadOnlyDictionary<string, object> eventProperties)
        {
            if (_ignoredEvents.Contains(eventName)) return;

            if (eventProperties == null || eventProperties.Count == 0)
            {
                OnEventLogged(eventName);
                return;
            }
            
            var data = new Dictionary<string, string>();
            foreach (var kvp in eventProperties)
            {
                var value = kvp.Value?.ToString();
                if (string.IsNullOrEmpty(value)) continue;
                
                data[kvp.Key] = value;
            }

            SentrySdk.AddBreadcrumb($"Amplitude event logged: {eventName}", "amplitude.event", data: data);
        }
    }
}