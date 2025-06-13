using System;
using Bridge.Models.VideoServer;

namespace UIManaging.Pages.RatingFeed.Rating
{
    internal sealed class RatingVideo
    {
        public Video Video { get; }
        public VideoRating Rating { get; set; }
        
        public event Action<RatingVideo> RatingChanged;
        
        public RatingVideo(Video video, VideoRating videoRating)
        {
            Video = video;
            Rating = videoRating;
            
            Rating.ScoreChanged += OnScoreChanged;
        }

        private void OnScoreChanged(int _) => RatingChanged?.Invoke(this);
    }
}