using System.Linq;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;

namespace Common.ExceptionCatcher
{
    internal sealed class InformativeExceptionHandler: ExceptionHandler
    {
        private static readonly string[] INFORMATIVE_EXCEPTION  = 
        {
            Constants.ErrorMessage.BROKEN_CHARACTER
        };

        public InformativeExceptionHandler(ExceptionsAnalyticsService analyticsService, PopupManager popupManager) : base(analyticsService, popupManager)
        {
        }

        public override bool IsTarget(ErrorContext errorContext)
        {
            return INFORMATIVE_EXCEPTION.Any(errorContext.Condition.Contains);
        }

        public override void Process(ErrorContext errorContext)
        {
            var popupConfig = GetPopupConfig(errorContext.Condition);
            DisplayPopup(popupConfig);
            SendToAnalytics(errorContext.Condition, errorContext.StackTrace, true);
        }
        
        private static PopupConfiguration GetPopupConfig(string errorMessage)
        {
            var config = new AlertPopupConfiguration
            {
                PopupType = PopupType.AlertPopup,
                Description = INFORMATIVE_EXCEPTION.First(errorMessage.Contains),
                ConfirmButtonText = "Ok"
            };

            return config;
        }
    }
}