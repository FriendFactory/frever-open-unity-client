using System.Collections.Generic;
using Extensions;
using Modules.Amplitude;
using Modules.Amplitude.Events.Core;

namespace UIManaging.Pages.RatingFeed.Amplitude
{
    internal class VideoRatingCompletedAmplitudeEvent: BaseAmplitudeEvent
    {
        public override string Name => "video_rating_completed";

        public VideoRatingCompletedAmplitudeEvent()
        {
        }

        public VideoRatingCompletedAmplitudeEvent(IEnumerable<VideoRatingEventData> videoRatingEvents)
        {
            var ids = new List<long>();
            var ratings = new List<int>();
            var votingTimes = new List<float>();
            var timestamps = new List<string>();

            videoRatingEvents.ForEach(videoRating =>
            {
                ids.Add(videoRating.VideoId);
                timestamps.Add(videoRating.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.ffffff"));
                ratings.Add(videoRating.Rating);
                votingTimes.Add(videoRating.VotingTime);
            });
            
            _eventProperties.Add(AmplitudeEventConstants.EventProperties.VIDEO_ID, ids);
            _eventProperties.Add(AmplitudeEventConstants.EventProperties.TIMESTAMP, timestamps);
            _eventProperties.Add(AmplitudeEventConstants.EventProperties.RATING, ratings);
            _eventProperties.Add("Voting Time", votingTimes);
        }
    }
}