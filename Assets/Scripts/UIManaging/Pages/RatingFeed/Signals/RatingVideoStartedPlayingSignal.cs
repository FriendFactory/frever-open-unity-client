using System;
using UIManaging.Pages.RatingFeed.Rating;

namespace UIManaging.Pages.RatingFeed.Signals
{
    internal sealed class RatingVideoStartedPlayingSignal
    {
        public RatingVideo RatingVideo { get; }
        public DateTime Timestamp { get; }

        public RatingVideoStartedPlayingSignal(RatingVideo ratingVideo, DateTime timestamp)
        {
            RatingVideo = ratingVideo;
            Timestamp = timestamp;
        }
    }
}