using Navigation.Core;
using RenderHeads.Media.AVProVideo;

namespace UiManaging.Pages.OnBoardingPage.UI.Args
{
    public class OnBoardingPageArgs : OnBoardingBasePageArgs
    {
        public override PageId TargetPage { get; } = PageId.OnBoardingPage;

        public MediaPathType FileLocation { get; } = MediaPathType.AbsolutePathOrURL;
        public bool AutoPlay { get; } = true;

        // TODO: temporary hardcode video link to fix issue with audio not mutable in AVPro Video v2.2.1
        // public string VideoUrl { get; } = "https://ff-publicfiles.s3.eu-central-1.amazonaws.com/video_raw.mp4";
        public string VideoUrl { get; } = "https://ff-publicfiles.s3.eu-central-1.amazonaws.com/login-bg-608x1080.mp4";
        public bool ShowEnvironmentSelection { get; set; }
    }
}