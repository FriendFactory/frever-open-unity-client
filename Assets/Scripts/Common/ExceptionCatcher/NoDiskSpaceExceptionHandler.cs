using System;
using System.Collections.Generic;
using Bridge;
using Extensions;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace Common.ExceptionCatcher
{
    internal sealed class NoDiskSpaceExceptionHandler: ExceptionHandler
    {
        private const string ERROR_KEYWORD = "Disk full";
        private readonly IBridge _bridge;

        public NoDiskSpaceExceptionHandler(ExceptionsAnalyticsService analyticsService, PopupManager popupManager, IBridge bridge) : base(analyticsService, popupManager)
        {
            _bridge = bridge;
        }

        public override bool IsTarget(ErrorContext errorContext)
        {
            return errorContext.Condition.Contains(ERROR_KEYWORD);
        }

        public override void Process(ErrorContext errorContext)
        {
            var popupConfig = GetPopupConfiguration();
            DisplayPopup(popupConfig);
            SendToAnalytics(errorContext.Condition, errorContext.StackTrace, true);
        }

        private PopupConfiguration GetPopupConfiguration()
        {
            var variants = new List<KeyValuePair<string, Action>>
            {
                new KeyValuePair<string, Action>("Clear cache and restart", ClearCacheAndQuit),
                new KeyValuePair<string, Action>("Quit the app", Quit)
            };
            return new ActionSheetPopupConfiguration
            {
                PopupType = PopupType.ActionSheet,
                Description = "Disk space on your phone is full",
                Variants = variants
            };
        }

        private async void ClearCacheAndQuit()
        {
            await _bridge.ClearCacheWithoutKeyFileStorage();
            Quit();
        }

        private void Quit()
        {
            Application.Quit();
        }
    }
}