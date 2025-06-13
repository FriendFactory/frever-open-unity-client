using System;
using System.Threading;
using Bridge.Models.VideoServer;
using UIManaging.Pages.Common.VideoManagement;

namespace Navigation.Args.Feed
{
    public class TrendingFeedArgs : BaseFeedArgs
    {
        public override string Name => "Trending";
        public override VideoListType VideoListType => VideoListType.Trending;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public TrendingFeedArgs(VideoManager videoManager, long? idOfFirstVideoToShow) : base(videoManager, idOfFirstVideoToShow)
        {
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override bool ShouldShowNavigationBar()
        {
            return false;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void DownloadVideosInternal(Action<Video[]> onSuccess, Action<string> onFail, long? videoId, int takeNextCount, int takePreviousCount = 0, CancellationToken cancellationToken = default)
        {
            VideoManager.GetFeedVideos(onSuccess, onFail, VideoListType, videoId, takeNextCount, takePreviousCount, cancellationToken);
        }

        protected override void OnVideosDownloaded(Video[] videos, Action<Video[]> callback)
        {
            callback?.Invoke(videos);
        }
    }
}