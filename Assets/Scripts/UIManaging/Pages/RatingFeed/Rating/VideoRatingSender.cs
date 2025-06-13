using System;
using System.Threading.Tasks;
using Bridge;
using Bridge.VideoServer;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.RatingFeed.Rating
{
    [UsedImplicitly]
    internal sealed class VideoRatingSender : IInitializable, IDisposable
    {
        private const int MAX_RETRY_ATTEMPTS = 3;
        private const int RETRY_DELAY = 5;

        private readonly IVideoBridge _videoBridge;
        private readonly RatingFeedViewModel _ratingFeedViewModel;
        private readonly RatingFeedPageModel _pageModel;

        private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(RETRY_DELAY);

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private long LevelId => _ratingFeedViewModel.Level.Id;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public VideoRatingSender(IVideoBridge videoBridge, RatingFeedViewModel ratingFeedViewModel, RatingFeedPageModel pageModel)
        {
            _videoBridge = videoBridge;
            _ratingFeedViewModel = ratingFeedViewModel;
            _pageModel = pageModel;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Initialize()
        {
            _pageModel.RatingStarted += OnRatingStarted;
        }

        public void Dispose()
        {
            _pageModel.RatingStarted -= OnRatingStarted;
            _ratingFeedViewModel.RatingVideos?.ForEach(ratingVideo => ratingVideo.RatingChanged -= OnRatingChanged);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnRatingStarted()
        {
            _pageModel.RatingStarted += OnRatingStarted;

            _ratingFeedViewModel.RatingVideos.ForEach(ratingVideo => ratingVideo.RatingChanged += OnRatingChanged);
        }

        private async void OnRatingChanged(RatingVideo ratingVideo)
        {
            try
            {
                await RateVideoWithRetryAsync(ratingVideo, MAX_RETRY_ATTEMPTS, _retryDelay);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private async Task RateVideoWithRetryAsync(RatingVideo ratingVideo, int maxAttempts, TimeSpan delay)
        {
            var attempt = 0;

            while (attempt < maxAttempts)
            {
                attempt++;

                try
                {
                    await RateVideoAsync(ratingVideo);
                    return;
                }
                catch (VideoRatingException ex)
                {
                    if (attempt >= maxAttempts)
                    {
                        Debug.LogError($"Failed to rate video after {maxAttempts} attempts. Error: {ex.Message}");
                    }

                    Debug.LogWarning($"Rating attempt {attempt} failed: {ex.Message}. Retrying in {delay.Seconds} seconds...");
                    await Task.Delay(delay);
                }
            }
        }

        private async Task RateVideoAsync(RatingVideo ratingVideo)
        {
            var requestModel = new RateVideoRequest
            {
                RaterLevelId = LevelId,
                RatedVideoId = ratingVideo.Video.Id,
                Rating = ratingVideo.Rating.Score,
            };

            var result = await _videoBridge.RateVideoList(requestModel);
            if (result.IsError)
            {
                if (result.ErrorMessage != null &&
                    result.ErrorMessage.Contains("ERROR_VIDEO_ALREADY_RATED"))
                {
                    Debug.LogError($"Error while rating video: {result.ErrorMessage}");
                }
                else
                {
                    throw new VideoRatingException(result.ErrorMessage);
                }
            }
        }
    }

    //---------------------------------------------------------------------
    // Nested
    //---------------------------------------------------------------------

    internal class VideoRatingException : Exception
    {
        public VideoRatingException(string message) : base(message)
        {
        }
    }
}