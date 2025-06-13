namespace UIManaging.Pages.SeasonPage
{
    internal sealed class SeasonRewardsLevelModel : SeasonRewardsItemModel
    {
        public long Id { get; }
        public int Level { get; }

        public bool IsFirst { get; }
        public bool IsLast { get; }
        public bool CanBeClaimed { get; set; }
        public bool IsPremium { get; }

        public SeasonRewardWrapper FreeReward { get; }
        public SeasonRewardWrapper PremiumReward { get; }
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public SeasonRewardsLevelModel(long id, int level, SeasonRewardWrapper freeReward, SeasonRewardWrapper premiumReward,
            bool isFirst, bool isLast, bool isPremium, bool canBeClaimed)
        {
            Id = id;
            Level = level;
            
            FreeReward = freeReward;
            PremiumReward = premiumReward;
            
            IsFirst = isFirst;
            IsLast = isLast;
            IsPremium = isPremium;
            
            CanBeClaimed = canBeClaimed;
        }
    }
}