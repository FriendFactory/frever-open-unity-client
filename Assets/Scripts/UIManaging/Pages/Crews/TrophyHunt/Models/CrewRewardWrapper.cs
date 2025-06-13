using Bridge.Models.ClientServer.Crews;
using Bridge.Models.ClientServer.Gamification.Reward;
using UIManaging.Common.Rewards;

namespace UIManaging.Pages.Crews.TrophyHunt.Models
{
    public class CrewRewardWrapper
    {
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public CrewReward Reward { get; }
        public RewardType Type { get; }
        public RewardState State { get; set; }

        public long Id => Reward.Id;
        public int? SoftCurrency => Reward.SoftCurrency;
        public int? HardCurrency => Reward.HardCurrency;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public CrewRewardWrapper(CrewReward reward, RewardState state)
        {
            Reward = reward;
            Type = reward.GetRewardType();
            State = state;
        }
    }
}