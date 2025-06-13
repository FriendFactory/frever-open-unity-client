using System.Linq;
using Bridge.Authorization.Models;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace Common.ExceptionCatcher
{
    internal abstract class ExceptionHandler
    {
        private readonly string[] _redundantStackTraceSymbols = 
        { 
            "(at <00000000000000000000000000000000>:0)",
            "System.Threading.ContextCallback.Invoke (System.Object state)" ,
            "System.Threading.ExecutionContext.RunInternal (System.Threading.ExecutionContext executionContext, System.Threading.ContextCallback callback, System.Object state, System.Boolean preserveSyncCtx)",
            "System.Threading.Tasks.AwaitTaskContinuation.RunCallback (System.Threading.ContextCallback callback, System.Object state, System.Threading.Tasks.Task& currentTask)"
        };
        
        private readonly ExceptionsAnalyticsService _analyticsService;
        private readonly PopupManager _popupManager;

        protected ExceptionHandler(ExceptionsAnalyticsService analyticsService, PopupManager popupManager)
        {
            _analyticsService = analyticsService;
            _popupManager = popupManager;
        }

        public virtual bool TriggerCaughtEvent => false;

        public abstract bool IsTarget(ErrorContext errorContext);

        public abstract void Process(ErrorContext errorContext);

        protected void SendToAnalytics(string condition, string stackTrace, bool handled, bool isTimeoutError = false)
        {
            _analyticsService.SendErrorMessageAmplitudeEvent(condition, CleanupStackTrace(stackTrace), handled, isTimeoutError);
        }

        protected void DisplayPopup(PopupConfiguration config)
        {
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
        }
        
        private string CleanupStackTrace(string stackTrace)
        {
            return _redundantStackTraceSymbols.Aggregate(stackTrace, (current, symbol) => current.Replace(symbol, string.Empty));
        }
    }

    internal struct ErrorContext
    {
        public readonly string Condition;
        public readonly string StackTrace;
        public readonly UserProfile User;
        public readonly LogType LogType;

        public ErrorContext(string condition, string stackTrace, UserProfile user, LogType logType)
        {
            Condition = condition;
            StackTrace = stackTrace;
            User = user;
            LogType = logType;
        }
    }
}