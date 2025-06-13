using Bridge;
using Bridge.Exceptions;
using Extensions;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace Common.ExceptionCatcher
{
    internal sealed class FileEncryptionErrorHandler: ExceptionHandler
    {
        private static readonly string TARGET_PREFIX = FileEncryptionException.ERROR_PREFIX;
        private static readonly string STACKTRACE_PREFIX = $"Rethrow as {nameof(FileEncryptionException)}";
        private const float POPUP_DISPLAY_PERIOD = 5f;
            
        private readonly IBridge _bridge;

        private float _nextPopupShownTime;
        
        public FileEncryptionErrorHandler(ExceptionsAnalyticsService analyticsService, PopupManager popupManager,
            IBridge bridge) : base(analyticsService, popupManager)
        {
            _bridge = bridge;
        }

        public override bool TriggerCaughtEvent => true;

        public override bool IsTarget(ErrorContext errorContext)
        {
            return IsErrorTarget() || IsExceptionTarget();

            bool IsErrorTarget() => errorContext.LogType == LogType.Error && errorContext.Condition.StartsWith(TARGET_PREFIX);

            bool IsExceptionTarget() => errorContext.LogType == LogType.Exception &&
                                        (errorContext.Condition.StartsWith(nameof(FileEncryptionException)) ||
                                         errorContext.StackTrace.StartsWith(STACKTRACE_PREFIX));
        }

        public override void Process(ErrorContext errorContext)
        {
            SendToAnalytics(errorContext.Condition, errorContext.StackTrace, false);

            // user could close popup and receive N errors in a row
            if (Time.time < _nextPopupShownTime) return;

            _nextPopupShownTime = Time.time + POPUP_DISPLAY_PERIOD; 
            
            var config = GetPopupConfiguration();
            DisplayPopup(config);
        }
        
        private PopupConfiguration GetPopupConfiguration()
        {
            var config = new AlertPopupConfiguration()
            {
                PopupType = PopupType.FilesInconsistency,
                OnConfirm = ClearCacheAndQuit,
                Title = "File inconsistency",
                Description = "Sorry, some file inconsistency has happened, please clear cache and restart the app by clicking the button below",
                ConfirmButtonText = "Clear cache and quit",
            };

            async void ClearCacheAndQuit()
            {
                await _bridge.ClearCacheWithoutKeyFileStorage();
                Application.Quit();
            }

            return config;
        }
        
    }
}