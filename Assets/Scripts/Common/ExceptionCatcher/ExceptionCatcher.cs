using System;
using Bridge;
using Bridge.Authorization.Models;
using UIManaging.PopupSystem;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;

namespace Common.ExceptionCatcher
{
    internal sealed class ExceptionCatcher : MonoBehaviour, IExceptionCatcher
    {
        [Inject] private PopupManager _popupManager;
        [Inject] private IgnoredExceptionsData _ignoredExceptions;
        [Inject] private ExceptionsAnalyticsService _analyticsService;
        [Inject] private IBridge _bridge;
        [Inject] private SnackBarHelper _snackBarHelper;

        private UserProfile UserProfile => _bridge.Profile;

        private ExceptionHandler[] _handlers;

        public event Action ExceptionCaught;
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _handlers = new ExceptionHandler[]
            {
                //order matters, default must be at the end
                new BadGatewayErrorHandler(_analyticsService, _popupManager, ExceptionCaught),
                new LowDiskSpaceExceptionHandler(_analyticsService, _popupManager),
                new NoDiskSpaceExceptionHandler(_analyticsService, _popupManager, _bridge),
                new InformativeExceptionHandler(_analyticsService, _popupManager),
                new TimeOutExceptionHandler(_analyticsService, _popupManager),
                new ForbiddenErrorHandler(_analyticsService, _popupManager, _snackBarHelper),
                new ARSessionErrorsHandler(_analyticsService, _popupManager),
                new FileEncryptionErrorHandler(_analyticsService, _popupManager, _bridge),
                // may intersect with the handlers above, so, putted here
                new IgnoredExceptionHandler(_analyticsService, _popupManager, _ignoredExceptions),
                new DefaultErrorLogTypeHandler(_analyticsService, _popupManager),
                new DefaultExceptionLogTypeHandler(_analyticsService, _popupManager)
            };
        }

        private void OnEnable()
        {
            Application.logMessageReceived += OnLogMessageReceived;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
        }

        public void TryCatchBlockTriggered()
        {
            ExceptionCaught?.Invoke();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception) return;

            var context = new ErrorContext(condition, stackTrace, UserProfile, type);
            foreach (var handler in _handlers)
            {
                if (!handler.IsTarget(context)) continue;
                if (IsCustomHandler(handler) && handler.TriggerCaughtEvent) ExceptionCaught?.Invoke();  
                
                handler.Process(context);
                break;
            }
        }

        private static bool IsCustomHandler(ExceptionHandler handler)
        {
            return handler.GetType() == typeof(IgnoredExceptionHandler) ||
                   handler.GetType() == typeof(DefaultErrorLogTypeHandler) ||
                   handler.GetType() == typeof(DefaultExceptionLogTypeHandler);
        }
    }
}