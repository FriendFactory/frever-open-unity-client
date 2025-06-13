using Common.Exceptions;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace Common.ExceptionCatcher
{
    internal sealed class LowDiskSpaceExceptionHandler : ExceptionHandler
    {
        public LowDiskSpaceExceptionHandler(ExceptionsAnalyticsService analyticsService, PopupManager popupManager) :
            base(analyticsService, popupManager)
        {
        }

        public override bool IsTarget(ErrorContext errorContext)
        {
            return errorContext.Condition.Contains(ErrorConstants.LOW_DISK_SPACE_ERROR_MESSAGE);
        }

        public override void Process(ErrorContext errorContext)
        {
            var popupConfig = GetPopupConfiguration();
            DisplayPopup(popupConfig);
            SendToAnalytics(errorContext.Condition, errorContext.StackTrace, true);
        }

        private PopupConfiguration GetPopupConfiguration()
        {
            return new AlertPopupConfiguration
            {
                PopupType  = PopupType.AlertWithTitlePopup,
                ConfirmButtonText = "Open Settings",
                Description = "Disk space on your phone is full. Please clean up the storage before continue playing",
                Title = "Oops",
                OnConfirm = OpenNativeGeneralSettingsMenu
            };
        }

        private void OpenNativeGeneralSettingsMenu()
        {
            #if UNITY_EDITOR
            Debug.Log($"Frever: Opening settings app requests.");
            #elif UNITY_IOS
            Application.OpenURL("app-settings:");
            #elif UNITY_ANDROID
            var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var unityActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
            var context = unityActivity.Call<AndroidJavaObject>("getApplicationContext");

            var settingsClass = new AndroidJavaClass("android.provider.Settings");
            var intent =
                new AndroidJavaObject("android.content.Intent", settingsClass.GetStatic<string>("ACTION_SETTINGS"));
            intent.Call<AndroidJavaObject>("addFlags", intent.GetStatic<int>("FLAG_ACTIVITY_NEW_TASK"));
            context.Call("startActivity", intent);
            #endif
        }
    }
}