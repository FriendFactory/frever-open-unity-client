using System;
using System.Collections.Generic;
using System.Diagnostics;
using Extensions;
using JetBrains.Annotations;
using Modules.Amplitude.Signals;
using UIManaging.Pages.RatingFeed.Rating;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.RatingFeed.Amplitude
{
    [UsedImplicitly]
    internal class VideoRatingAmplitudeEventSignalEmitter: BaseAmplitudeEventSignalEmitter
    {
        private readonly RatingFeedViewModel _ratingFeedViewModel;
        private readonly RatingFeedPageModel _pageModel;
        private readonly VideoRatingAmplitudeEventsFileHandler _fileHandler;
        
        private readonly Stopwatch _stopwatch = new ();
        private readonly List<VideoRatingEventData> _videoRatingEvents = new ();

        public VideoRatingAmplitudeEventSignalEmitter(SignalBus signalBus, RatingFeedViewModel ratingFeedViewModel,
            RatingFeedPageModel pageModel, VideoRatingAmplitudeEventsFileHandler fileHandler) : base(signalBus)
        {
            _ratingFeedViewModel = ratingFeedViewModel;
            _pageModel = pageModel;
            _fileHandler = fileHandler;
        }
        
        public override void Initialize()
        {
            _pageModel.RatingStarted += OnRatingStarted;

            _ratingFeedViewModel.RatingCancelled += OnRatingCancelled;
        }

        public override void Dispose()
        {
            _pageModel.RatingStarted -= OnRatingStarted;
            
            _ratingFeedViewModel.RatingCancelled -= OnRatingCancelled;
            
            StopRatingChangedTracking();
        }

        private void OnRatingStarted()
        {
            Emit(new VideoRatingStartedAmplitudeEvent(_ratingFeedViewModel.Level.Id, _ratingFeedViewModel.RatingVideos));
            
            StartRatingChangedTracking();
        }

        private void OnScoreChanged(RatingVideo ratingVideo)
        {
            var votingTime = Mathf.Min((float)_stopwatch.Elapsed.TotalSeconds, RatingFeedConstants.RATING_TIMEOUT);
            var timestamp = DateTime.UtcNow;
            
            _stopwatch.Restart();
            
            var videoRatingEvent = new VideoRatingEventData(ratingVideo.Video.Id, ratingVideo.Rating.Score, votingTime, timestamp);
            
            _videoRatingEvents.Add(videoRatingEvent);
        }

        private void OnRatingCompleted()
        {
            StopRatingChangedTracking();
            
            Emit(new VideoRatingCompletedAmplitudeEvent(_videoRatingEvents));
            
            _stopwatch.Stop();
            
            _videoRatingEvents.Clear();
        }

        private void OnRatingCancelled(VideoRatingCancellationReason reason)
        {
            if (_videoRatingEvents.IsNullOrEmpty()) return;
            
            var videoRatingCancelledAmplitudeEvent = new VideoRatingCancelledAmplitudeEvent(_videoRatingEvents, reason);
            
            if (reason == VideoRatingCancellationReason.Skipped)
            {
                Emit(videoRatingCancelledAmplitudeEvent);
            }
            else
            {
                _fileHandler.Save(videoRatingCancelledAmplitudeEvent);
            }
        }

        private void StartRatingChangedTracking()
        {
            _pageModel.RatingStarted -= OnRatingStarted;
            
            _ratingFeedViewModel.RatingFeedProgress.Completed += OnRatingCompleted;
            
            _ratingFeedViewModel.RatingVideos.ForEach(video => video.RatingChanged += OnScoreChanged);
            
            _stopwatch.Start();
        }

        private void StopRatingChangedTracking()
        {
            _ratingFeedViewModel.RatingFeedProgress.Completed -= OnRatingCompleted;
            _ratingFeedViewModel.RatingVideos?.ForEach(video => video.RatingChanged -= OnScoreChanged);
        }
    }
}