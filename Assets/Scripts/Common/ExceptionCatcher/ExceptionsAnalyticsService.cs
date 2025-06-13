using System;
using System.Collections.Generic;
using Modules.Amplitude;

namespace Common.ExceptionCatcher
{
    internal sealed class ExceptionsAnalyticsService
    {
        private const int ERROR_WAIT_PERIOD = 5;
        
        private readonly AmplitudeManager _amplitudeManager;
        private readonly Dictionary<string, DateTime> _cachedErrors = new Dictionary<string, DateTime>();

        public ExceptionsAnalyticsService(AmplitudeManager amplitudeManager)
        {
            _amplitudeManager = amplitudeManager;
        }

        public void SendErrorMessageAmplitudeEvent(string errorType, string stackTrace, bool handled, bool isTimeoutError)
        {
            if (IsErrorRecentlySent(stackTrace)) return;

            var errorMetaData = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.ERROR_TYPE] = errorType,
                [AmplitudeEventConstants.EventProperties.ERROR_STACK_TRACE] = stackTrace,
                [AmplitudeEventConstants.EventProperties.ERROR_HANDLED] = handled,
                [AmplitudeEventConstants.EventProperties.SLOW_CONNECTION_POPUP] = isTimeoutError
            };
            
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.ERROR_MESSAGE, errorMetaData);
            CacheError(stackTrace);
        }

        private bool IsErrorRecentlySent(string stackTrace)
        {
            return _cachedErrors.ContainsKey(stackTrace) &&
                   DateTime.Now.Subtract(_cachedErrors[stackTrace]).Seconds < ERROR_WAIT_PERIOD;
        }

        private void CacheError(string stackTrace)
        {
            _cachedErrors[stackTrace] =  DateTime.Now;
        }
    }
}