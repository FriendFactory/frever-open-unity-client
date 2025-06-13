namespace UIManaging.PopupSystem.Configurations
{
    public sealed class VideoUploadingCountdownPopupConfiguration : PopupConfiguration
    {
        public readonly float InitialProgressValue;
        public VideoUploadingCountdownPopupConfiguration(float initialProgressValue)
        {
            InitialProgressValue = initialProgressValue;
            PopupType = PopupType.VideoUploadingCountdown;
        }
    }
}