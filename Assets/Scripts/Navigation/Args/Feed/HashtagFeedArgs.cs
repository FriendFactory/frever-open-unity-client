using System;
using System.Threading;
using Bridge.Models.VideoServer;
using Bridge;
using UIManaging.Pages.Common.VideoManagement;

namespace Navigation.Args.Feed
{
    public sealed class HashtagFeedArgs : VideosBasedOnTemplateFeedArgs
    {
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public HashtagInfo HashtagInfo { get; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public HashtagFeedArgs(HashtagInfo hashtagInfo, VideoManager videoManager, IBridge bridge)
            : base (videoManager, -1)
        {
            HashtagInfo = hashtagInfo;
        }
        
        public HashtagFeedArgs(HashtagInfo hashtagInfo, VideoManager videoManager, long idOfFirstVideoToShow, int indexOfFirstVideoToShow)
            : base(videoManager, idOfFirstVideoToShow, indexOfFirstVideoToShow, -1)
        {
            HashtagInfo = hashtagInfo;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override bool ShouldShowTabs()
        {
            return false;
        }

        public override bool ShouldShowNavigationBar()
        {
            return false;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void DownloadVideosInternal(Action<Video[]> onSuccess, Action<string> onFail, long? videoId, int takeNextCount, int takePreviousCount = 0, CancellationToken cancelationToken = default)
        {
            if (takeNextCount == 0) return;
            VideoManager.GetHashtagVideos(HashtagInfo.Id, onSuccess, onFail, videoId, takeNextCount, takePreviousCount, cancelationToken);
        }

        protected override void OnVideosDownloaded(Video[] videos, Action<Video[]> callback)
        {
            callback?.Invoke(videos);
        }
    }
}