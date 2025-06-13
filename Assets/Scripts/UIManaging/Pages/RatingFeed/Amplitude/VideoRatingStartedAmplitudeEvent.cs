using System.Collections.Generic;
using System.Linq;
using Modules.Amplitude;
using Modules.Amplitude.Events.Core;
using UIManaging.Pages.RatingFeed.Rating;

namespace UIManaging.Pages.RatingFeed.Amplitude
{
    internal class VideoRatingStartedAmplitudeEvent: BaseAmplitudeEvent
    {
        public override string Name => "video_rating_started";
        
        public VideoRatingStartedAmplitudeEvent(long levelId, IEnumerable<RatingVideo> ratingVideos)
        {
            _eventProperties.Add(AmplitudeEventConstants.EventProperties.LEVEL_ID, levelId);
            _eventProperties.Add(AmplitudeEventConstants.EventProperties.VIDEO_ID, ratingVideos.Select(video => video.Video.Id).ToArray());
        }
    }
}