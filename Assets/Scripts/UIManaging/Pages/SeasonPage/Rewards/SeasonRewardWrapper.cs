using Bridge.Models.ClientServer.Gamification;
using Bridge.Models.ClientServer.Gamification.Reward;
using Modules.Amplitude;
using Zenject;

namespace UIManaging.Pages.SeasonPage
{
    public class SeasonRewardWrapper
    {
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public SeasonReward Reward { get; }
        public RewardType Type { get; }
        public bool IsClaimed { get; set; }

        public long Id => Reward.Id;
        public int? SoftCurrency => Reward.SoftCurrency;
        public int? HardCurrency => Reward.HardCurrency;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public SeasonRewardWrapper(SeasonReward reward, RewardType type, bool isClaimed)
        {
            Reward = reward;
            Type = type;
            IsClaimed = isClaimed;
        }
    }
}