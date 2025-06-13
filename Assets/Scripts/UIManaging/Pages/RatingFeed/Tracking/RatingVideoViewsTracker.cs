using System;
using System.Collections.Generic;
using Bridge.VideoServer;
using Extensions;
using JetBrains.Annotations;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.Pages.Feed.Ui.Feed;
using UIManaging.Pages.RatingFeed.Amplitude;
using UIManaging.Pages.RatingFeed.Signals;
using Zenject;

namespace UIManaging.Pages.RatingFeed.Tracking
{
    [UsedImplicitly]
    internal sealed class RatingVideoViewsTracker : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly RatingFeedViewModel _ratingFeedViewModel;
        private readonly RatingFeedProgress _ratingFeedProgress;
        private readonly RatingVideoViewsFileHandler _fileHandler;
        private readonly VideoViewSender _videoViewSender;

        private readonly List<VideoView> _videoViews = new();

        public RatingVideoViewsTracker(SignalBus signalBus, RatingFeedViewModel ratingFeedViewModel,
            RatingFeedProgress ratingFeedProgress, RatingVideoViewsFileHandler fileHandler, VideoViewSender videoViewSender)
        {
            _signalBus = signalBus;
            _ratingFeedViewModel = ratingFeedViewModel;
            _fileHandler = fileHandler;
            _videoViewSender = videoViewSender;
            _ratingFeedProgress = ratingFeedProgress;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<RatingVideoStartedPlayingSignal>(OnRatingVideoStartedPlaying);

            _ratingFeedProgress.Completed += OnRatingCompleted;
            _ratingFeedViewModel.RatingCancelled += OnRatingCancelled;
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<RatingVideoStartedPlayingSignal>(OnRatingVideoStartedPlaying);
            
            _ratingFeedProgress.Completed -= OnRatingCompleted;
            _ratingFeedViewModel.RatingCancelled -= OnRatingCancelled;
        }

        private void OnRatingCancelled(VideoRatingCancellationReason reason)
        {
            if (_videoViews.IsNullOrEmpty()) return;

            switch (reason)
            {
                case VideoRatingCancellationReason.Skipped:
                    Send();
                    break;
                case VideoRatingCancellationReason.ApplicationQuit:
                    _fileHandler.Save(_videoViews);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(reason), reason, null);
            }
        }

        private void OnRatingCompleted() => Send();
        private void Send() => _videoViewSender.Send(_videoViews.ToArray());

        private void OnRatingVideoStartedPlaying(RatingVideoStartedPlayingSignal signal)
        {
            var videoId = signal.RatingVideo.Video.Id;
            var timestamp = signal.Timestamp;

            var videoView = new VideoView()
            {
                VideoId = videoId,
                ViewDate = timestamp,
                FeedTab = VideoListType.Voting.ToString(),
                FeedType = "Rating Feed",
            };

            _videoViews.Add(videoView);
        }
    }
}