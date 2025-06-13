using System;
using System.Threading;
using Bridge.Models.VideoServer;
using Bridge.NotificationServer;
using Navigation.Core;
using UIManaging.Pages.Common.VideoManagement;

namespace Navigation.Args.Feed
{
    public class NotificationFeedArgs : BaseFeedArgs
    {
        private readonly long _videoId;
        private readonly PageManager _pageManager;
        private Action<Video[]> _videoCallback;

        public override string Name => "Notifications";
        public override bool ShowQuestsButton => false;

        public NotificationFeedArgs(VideoManager videoManager, PageManager pageManager, long idOfFirstVideoToShow, CommentInfo commentInfo) : base(videoManager, idOfFirstVideoToShow, commentInfo)
        {
            _pageManager = pageManager;
            _videoId = idOfFirstVideoToShow;
        }

        protected override void DownloadVideosInternal(Action<Video[]> onSuccess, Action<string> onFail, long? videoId, int takeNextCount, int takePreviousCount = 0, CancellationToken cancelationToken = default)
        {
            _videoCallback = onSuccess;
            VideoManager.GetVideoForUser(_videoId, null, VideosDownloaded, cancelationToken);
        }

        private void VideosDownloaded(Video video)
        {
            _videoCallback?.Invoke(new[] {video});
        }

        public override bool ShouldShowNavigationBar()
        {
            return false;
        }

        public override bool ShouldRefreshOnVideoDeleted()
        {
            return false;
        }

        public override void OnVideoDeleted()
        {
            _pageManager.MoveBack();
        }
    }
}