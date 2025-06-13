using System;

namespace UIManaging.Pages.RatingFeed.Rating
{
    internal sealed class VideoRating
    {
        public event Action<int> ScoreChanged;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public int Score { get; private set; } = RatingFeedConstants.DEFAULT_RATING;
        public bool IsRated { get; private set; }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetScore(int score)
        {
            if (score is < 1 or > 5)
            {
                throw new ArgumentOutOfRangeException(nameof(score), "Score must be between 1 and 5.");
            }

            Score = score;
            IsRated = true;
            
            ScoreChanged?.Invoke(Score);
        }
    }
}