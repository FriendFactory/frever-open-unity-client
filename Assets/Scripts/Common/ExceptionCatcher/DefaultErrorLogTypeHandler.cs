using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace Common.ExceptionCatcher
{
    /// <summary>
    /// Handle errors where LogType = Error
    /// </summary>
    internal sealed class DefaultErrorLogTypeHandler: DefaultExceptionLogTypeHandler 
    {
        public DefaultErrorLogTypeHandler(ExceptionsAnalyticsService analyticsService, PopupManager popupManager)
            : base(analyticsService, popupManager) { }

        public override bool IsTarget(ErrorContext errorContext) => errorContext.LogType == LogType.Error;
    }
}