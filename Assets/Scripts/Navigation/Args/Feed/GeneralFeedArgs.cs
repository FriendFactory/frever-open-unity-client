using System;
using System.Threading;
using Bridge.Models.VideoServer;
using UIManaging.Pages.Common.VideoManagement;

namespace Navigation.Args.Feed
{
    public class GeneralFeedArgs : BaseFeedArgs
    {
        public GeneralFeedArgs(VideoManager videoManager) : base(videoManager)
        {
        }

        public GeneralFeedArgs(VideoManager videoManager, long idOfFirstVideoToShow) : base(videoManager, idOfFirstVideoToShow)
        {
        }
        
        protected override void DownloadVideosInternal(Action<Video[]> onSuccess, Action<string> onFail, long? videoId, int takeNextCount, int takePreviousCount = 0, CancellationToken cancellationToken = default)
        {
            VideoManager.GetFeedVideos(onSuccess, onFail, VideoListType, videoId, takeNextCount, takePreviousCount, cancellationToken);
        }

        public override string Name => "General";

        public override bool ShouldShowTabs()
        {
            return true;
        }

        public override bool ShouldShowNotificationButton()
        {
            return true;
        }

        public override bool ShouldShowDiscoveryButton()
        {
            return true;
        }

        protected override Video[] OnBeforeVideosCallback(Video[] inputVideos)
        {
            return inputVideos;
        }
    }
}