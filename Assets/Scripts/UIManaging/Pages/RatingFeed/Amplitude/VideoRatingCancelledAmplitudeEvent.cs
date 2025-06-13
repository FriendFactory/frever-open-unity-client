using System.Collections.Generic;

namespace UIManaging.Pages.RatingFeed.Amplitude
{
    internal enum VideoRatingCancellationReason
    {
        Skipped,
        AlreadyRated,
        VideosUnavailable,
        ApplicationQuit,
    }
    
    internal sealed class VideoRatingCancelledAmplitudeEvent: VideoRatingCompletedAmplitudeEvent
    {
        public override string Name => "video_rating_canceled";

        public VideoRatingCancelledAmplitudeEvent()
        {
        }

        public VideoRatingCancelledAmplitudeEvent(IEnumerable<VideoRatingEventData> videoRatingEvents, VideoRatingCancellationReason reason) : base(videoRatingEvents)
        {
            _eventProperties.Add("Reason", reason.ToString());
        }
    }
}