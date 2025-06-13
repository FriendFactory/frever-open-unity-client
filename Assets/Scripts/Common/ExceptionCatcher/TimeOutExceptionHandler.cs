using System.Linq;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;

namespace Common.ExceptionCatcher
{
    /// <summary>
    /// Display time out popup and send data to analytics
    /// </summary>
    internal sealed class TimeOutExceptionHandler: ExceptionHandler
    {
        private static readonly string[] TIME_OUT_EXCEPTIONS  = 
        {
            // Substrings in timeout exceptions we want to catch
            "Timed Out",
            "Time Out",
            "time out",
            "timeout",
            "SocketException",
            "Connection",
            "Could not resolve host 'content-prod.frever-api.com'",
            "TlsNoCloseNotifyException",
            "TlsException",
            "TlsFatalAlert"
        };

        public TimeOutExceptionHandler(ExceptionsAnalyticsService analyticsService, PopupManager popupManager) 
            : base(analyticsService, popupManager)
        {
        }

        public override bool IsTarget(ErrorContext errorContext)
        {
            return TIME_OUT_EXCEPTIONS.Any(errorContext.Condition.Contains);
        }

        public override void Process(ErrorContext errorContext)
        {
            DisplayPopup(GetPopupConfig());
            SendToAnalytics(errorContext.Condition, errorContext.StackTrace, true, true);
        }
        
        private PopupConfiguration GetPopupConfig()
        {
            var config = new InformationPopupConfiguration()
            {
                PopupType = PopupType.SlowConnection
            };

            return config;
        }
    }
}