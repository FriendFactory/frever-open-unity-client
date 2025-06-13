using System;
using UIManaging.PopupSystem.Configurations;

namespace UIManaging.Common.UploadingPopups
{
    public sealed class VideoRenderingCountdownPopupConfiguration: PopupConfiguration
    {
        public readonly IVideoRenderingState VideoRenderingState;
        public readonly float FillProgressBarForVideoCapturing;
        public readonly string StartCaptureTitle;
        public readonly string RenderingVideoTitle;
        public readonly Action OnCancelClicked;

        public VideoRenderingCountdownPopupConfiguration(IVideoRenderingState renderingState, float fillProgressBarForVideoCapturing, string startCaptureTitle, string renderingVideoTitle, Action onCancelClicked)
        {
            VideoRenderingState = renderingState;
            FillProgressBarForVideoCapturing = fillProgressBarForVideoCapturing;
            StartCaptureTitle = startCaptureTitle;
            RenderingVideoTitle = renderingVideoTitle;
            OnCancelClicked = onCancelClicked;
            PopupType = PopupType.VideoRenderingCountdown;
        }
    }
}