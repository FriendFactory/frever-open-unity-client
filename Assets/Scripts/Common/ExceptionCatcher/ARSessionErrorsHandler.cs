using System.Linq;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;

namespace Common.ExceptionCatcher
{
    internal sealed class ARSessionErrorsHandler: ExceptionHandler
    {
        private static readonly string[] AR_SESSION_ERRORS =
        {
            "The session has failed with error code 102:"
        };

        public ARSessionErrorsHandler(ExceptionsAnalyticsService analyticsService, PopupManager popupManager) : base(analyticsService, popupManager) { }
        
        public override bool IsTarget(ErrorContext errorContext)
        {
            return AR_SESSION_ERRORS.Any(error => errorContext.Condition.StartsWith(error));
        }

        public override void Process(ErrorContext errorContext)
        {
            SendToAnalytics(errorContext.Condition, errorContext.StackTrace, true);
            
            var config = GetPopupConfiguration();
            DisplayPopup(config);
        }

        private PopupConfiguration GetPopupConfiguration()
        {
            var config = new AlertPopupConfiguration()
            {
                PopupType = PopupType.AlertWithTitlePopup,
                Title = "Error",
                Description = "Oops, looks like something is wrong with the camera, please restart your phone",
                ConfirmButtonText = "Ok",
            };

            return config;
        }
    }
}