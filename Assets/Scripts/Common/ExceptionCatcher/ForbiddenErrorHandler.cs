using UIManaging.PopupSystem;
using UIManaging.SnackBarSystem;

namespace Common.ExceptionCatcher
{
    internal sealed class ForbiddenErrorHandler : ExceptionHandler
    {
        private const string FORBIDDEN_ERROR = "403 Forbidden";
        private const string ERROR_MESSAGE = "Asset Unavailable";

        private readonly SnackBarHelper _snackBarHelper;
        
        public ForbiddenErrorHandler(ExceptionsAnalyticsService analyticsService, PopupManager popupManager, SnackBarHelper snackBarHelper) : base(analyticsService, popupManager)
        {
            _snackBarHelper = snackBarHelper;
        }

        public override bool IsTarget(ErrorContext errorContext)
        {
            return errorContext.Condition.Contains(FORBIDDEN_ERROR);
        }

        public override void Process(ErrorContext errorContext)
        {
            SendToAnalytics(errorContext.Condition, errorContext.StackTrace, true);
            _snackBarHelper.ShowInformationSnackBar(ERROR_MESSAGE);
        }
    }
}