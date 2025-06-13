using System;
using UIManaging.PopupSystem.Configurations;

namespace UIManaging.Pages.PublishPage
{
    public interface IPublishVideoPopupManager
    {
        void ShowVideoRenderingPopup(IVideoRenderingState videoRenderingState, bool isTaskVideo, Action onCanceled);
        void ShowVideoUploadingPopup(float initialProgressValue);
        void HideLoadingOverlay();
        void HideVideoRenderingCountdown();
        void ShowRestartRenderingOptionPopup(Action onRestartRenderingClicked, Action onCancelClicked, Action<object> onClose);
    }
}