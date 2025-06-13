using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bridge.Models.VideoServer;
using Navigation.Args.Feed;
using UIManaging.Pages.Feed.Core;

namespace UIManaging.Pages.Feed.Ui.Feed
{
    internal class FeedModelsDownloader
    {
        private const int VIDEOS_PER_REQUEST = 10;

        private readonly HashSet<long?> _activeDownloads = new HashSet<long?>();

        private BaseFeedArgs _pageArgs;
        private readonly FeedManagerView _feedManager;
        private CancellationTokenSource _cancellationSource;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public bool CancelledRequest => _cancellationSource?.IsCancellationRequested ?? false;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public FeedModelsDownloader(FeedManagerView feedManager)
        {
            _feedManager = feedManager;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void OnDisplayStart(BaseFeedArgs pageArgs)
        {
            _pageArgs = pageArgs;
            _cancellationSource = new CancellationTokenSource();

            _feedManager.OnScrolledToTop += DownloadPreviousVideos;
            _feedManager.OnScrolledAlmostToBottom += DownloadNextVideos;
            _feedManager.OnScroll += OnScrollPage;
        }

        public void OnHidingBegin()
        {
            if (_feedManager != null)
            {
                _feedManager.OnScrolledToTop -= DownloadPreviousVideos;
                _feedManager.OnScrolledAlmostToBottom -= DownloadNextVideos;
                _feedManager.OnScroll -= OnScrollPage;
            }

            _cancellationSource?.Cancel();
        }

        public void GetListOfVideos(long? videoId, Action<Video[]> onSuccess)
        {
            _pageArgs?.DownloadVideos(onSuccess, null, videoId, VIDEOS_PER_REQUEST, VIDEOS_PER_REQUEST / 2, _cancellationSource.Token);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void DownloadPreviousVideos()
        {
            var videoId = _feedManager.ContextData.FeedVideoModels.FirstOrDefault()?.Video.Id;
            if (!_activeDownloads.Add(videoId)) return;

            DownloadVideos(videoId, false);
        }

        private void DownloadNextVideos()
        {
            var videoId = _feedManager.ContextData.FeedVideoModels.LastOrDefault()?.Video.Id;
            if (!_activeDownloads.Add(videoId)) return;

            DownloadVideos(videoId, true);
        }

        private void DownloadVideos(long? videoId, bool isNextVideo)
        {
            DownloadVideosInternal(
                videos => OnScrollVideosDownloaded(videoId, videos, isNextVideo),
                message => OnVideosDownloadingFailed(videoId, message),
                videoId, isNextVideo ? VIDEOS_PER_REQUEST : 0, isNextVideo ? 0 : VIDEOS_PER_REQUEST);
        }

        private void DownloadVideosInternal(Action<Video[]> onSuccess, Action<string> onFail, long? videoId,
            int takeNextCount = 0, int takePreviousCount = 0)
        {
            _pageArgs.DownloadVideos(onSuccess, onFail, videoId, takeNextCount, takePreviousCount, _cancellationSource.Token);
        }

        private void OnScrollVideosDownloaded(long? videoId, IEnumerable<Video> videos, bool isNextVideo)
        {
            _activeDownloads.Remove(videoId);
            _feedManager.AddVideos(videos, isNextVideo);
        }

        private void OnVideosDownloadingFailed(long? videoId, string message)
        {
            _activeDownloads.Remove(videoId);
        }

        private void OnScrollPage()
        {
            _pageArgs.VideoModelIndex = _feedManager.VideoModelIndex;
            _pageArgs.VideoModelsCount = _feedManager.ContextData.FeedVideoModels.Count;
        }
    }
}