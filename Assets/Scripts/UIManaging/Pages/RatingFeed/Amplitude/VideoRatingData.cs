using System;

namespace UIManaging.Pages.RatingFeed.Amplitude
{
    internal sealed class VideoRatingEventData
    {
        public long VideoId { get; }
        public int Rating { get; }
        public float VotingTime { get; set; }
        public DateTime Timestamp { get; set; }
        
        public VideoRatingEventData(long videoId, int rating, float votingTime, DateTime timestamp)
        {
            VideoId = videoId;
            Rating = rating;
            VotingTime = votingTime;
            Timestamp = timestamp;
        }
    }
}