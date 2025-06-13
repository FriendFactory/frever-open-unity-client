namespace UIManaging.Pages.SeasonPage
{
    internal sealed class SeasonRepeatableBonusRewardsLevelModel 
    {
        public SeasonRepeatableBonusRewardsLevelModel(long id, int level, SeasonRewardWrapper reward)
        {
            Id = id;
            Level = level;
            Reward = reward;
        }

        public long Id { get; }
        public int Level { get; }
        public SeasonRewardWrapper Reward { get; }

    }
}