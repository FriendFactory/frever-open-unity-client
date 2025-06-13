namespace UIManaging.Pages.Common.VideoRating
{
    public sealed class VideoRatingTierModel
    {
        private const int GOLD_THRESHOLD = 35;
        private const int SILVER_THRESHOLD = 25;
        
        public int Rating { get; }
        public VideoRatingRewardTier Tier { get; }
        
        public VideoRatingTierModel(int rating)
        {
            Rating = rating;
            Tier = GetTier(rating);
        }

        private VideoRatingRewardTier GetTier(int votingReward)
        {
            return votingReward switch
            {
                >= GOLD_THRESHOLD => VideoRatingRewardTier.Gold,
                >= SILVER_THRESHOLD => VideoRatingRewardTier.Silver,
                _ => VideoRatingRewardTier.Bronze
            };
        }
    }
}