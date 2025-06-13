using System;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;

namespace Common.ExceptionCatcher
{
    internal sealed class BadGatewayErrorHandler : ExceptionHandler
    {
        private const string BAD_GATEWAY_ERROR = "502 Bad Gateway";
        private event Action _exceptionCaught;

        
        public BadGatewayErrorHandler(ExceptionsAnalyticsService analyticsService, PopupManager popupManager, Action exceptionCaught = null) : base(analyticsService, popupManager)
        {
            _exceptionCaught = exceptionCaught;
        }

        public override bool IsTarget(ErrorContext errorContext)
        {
            return errorContext.Condition.Contains(BAD_GATEWAY_ERROR);
        }

        public override void Process(ErrorContext errorContext)
        {
            SendToAnalytics(errorContext.Condition, errorContext.StackTrace, true);

            if (_exceptionCaught != null)
            {
                _exceptionCaught.Invoke();
                return;
            }
            
            var config = GetPopupConfiguration();
            DisplayPopup(config);
        }

        private PopupConfiguration GetPopupConfiguration()
        {
            var config = new InformationPopupConfiguration()
            {
                PopupType = PopupType.SlowConnection
            };

            return config;
        }
    }
}