using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace Common.ExceptionCatcher
{
    internal abstract class PopupShownExceptionHandler: ExceptionHandler
    {
        protected PopupShownExceptionHandler(ExceptionsAnalyticsService analyticsService, PopupManager popupManager) :
            base(analyticsService, popupManager) { }

        protected static PopupConfiguration GetPopupConfig(ErrorContext errorContext)
        {
            return errorContext.LogType == LogType.Exception ? GetExceptionPopupConfig() : GetErrorPopupConfig();
            
            PopupConfiguration GetExceptionPopupConfig()
            {
                var exceptionMessage = errorContext.Condition + "\n\n" + errorContext.StackTrace;

                return new ExceptionPopupConfiguration()
                {
                    PopupType = PopupType.Exception,
                    Description = exceptionMessage
                };
            }

            PopupConfiguration GetErrorPopupConfig() => new ExceptionPopupConfiguration
            {
                PopupType = PopupType.Fail,
                Description = errorContext.Condition
            };
        }
    }
}