using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace Common.ExceptionCatcher
{
    /// <summary>
    /// Handle errors where LogType = Exception
    /// </summary>
    internal class DefaultExceptionLogTypeHandler: PopupShownExceptionHandler 
    {
        public DefaultExceptionLogTypeHandler(ExceptionsAnalyticsService analyticsService, PopupManager popupManager) :
            base(analyticsService, popupManager) { }
        
        public override bool IsTarget(ErrorContext errorContext) => errorContext.LogType == LogType.Exception;

        public sealed override void Process(ErrorContext errorContext)
        {
            var user = errorContext.User;
            if (user != null && user.IsEmployee)
            {
                var popupConfig = GetPopupConfig(errorContext);
                DisplayPopup(popupConfig);
            }
            
            SendToAnalytics(errorContext.Condition, errorContext.StackTrace, false);
        }
    }
}