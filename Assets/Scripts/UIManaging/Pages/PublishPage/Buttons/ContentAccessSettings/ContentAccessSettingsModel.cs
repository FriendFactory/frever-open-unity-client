using Navigation.Args;

namespace UIManaging.Pages.PublishPage.Buttons
{
    public class ContentAccessSettingsModel
    {
        public bool ForceRemix { get; }
        public bool AllowRemix { get; }

        public ContentAccessSettingsModel(bool forceRemix, bool allowRemix)
        {
            ForceRemix = forceRemix;
            AllowRemix = allowRemix;
        }
    }

    public sealed class ContentAccessLevelBasedSettingsModel : ContentAccessSettingsModel
    {
        public VideoUploadingSettings VideoUploadingSettings { get; }

        public ContentAccessLevelBasedSettingsModel(bool forceRemix, bool allowRemix, VideoUploadingSettings videoUploadingSettings) : base(forceRemix, allowRemix)
        {
            VideoUploadingSettings = videoUploadingSettings;
        }
    }
}