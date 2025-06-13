using Bridge.Authorization.Models;
using UIManaging.PopupSystem;

namespace Common.ExceptionCatcher
{
    /// <summary>
    /// Don't show error for the user. Send to analytics if necessary
    /// </summary>
    internal sealed class IgnoredExceptionHandler: PopupShownExceptionHandler
    {
        private readonly IgnoredExceptionsData _ignoredExceptions;

        public IgnoredExceptionHandler(ExceptionsAnalyticsService analyticsService, PopupManager popupManager, IgnoredExceptionsData ignoredExceptions) : base(analyticsService, popupManager)
        {
            _ignoredExceptions = ignoredExceptions;
        }

        public override bool IsTarget(ErrorContext errorContext)
        {
            
            return ShouldBeIgnored(errorContext.Condition, errorContext.StackTrace, errorContext.User);
        }

        public override void Process(ErrorContext errorContext)
        {
            //TODO: merge all of these checks to single one, because formally IsIgnored check is enough to get all necessary data
            if (ShouldSendIgnoredErrorToAnalytics(errorContext.Condition, errorContext.StackTrace))
            {
                SendToAnalytics(errorContext.Condition, errorContext.StackTrace, true);
            }

            if (ShouldBeDisplayed(errorContext))
            {
                var popupConfig = GetPopupConfig(errorContext);
                DisplayPopup(popupConfig);
            }
        }
        
        private bool ShouldBeIgnored(string condition, string stackTrace, UserProfile userProfile)
        {
            return IsIgnored(condition, userProfile) || IsIgnored(stackTrace, userProfile);
        }

        private bool IsIgnored(string errorPart, UserProfile userProfile)
        {
            return _ignoredExceptions.ShouldBeIgnored(errorPart, userProfile);
        }
        
        private bool ShouldSendIgnoredErrorToAnalytics(string condition, string stackTrace)
        {
            return _ignoredExceptions.ShouldSendToAnalytics(condition) || _ignoredExceptions.ShouldSendToAnalytics(stackTrace);
        }

        private bool ShouldBeDisplayed(ErrorContext errorContext)
        {
            return _ignoredExceptions.ShouldBeDisplayed(errorContext.Condition, errorContext.User) ||
                   _ignoredExceptions.ShouldBeDisplayed(errorContext.StackTrace, errorContext.User);
        }
    }
}