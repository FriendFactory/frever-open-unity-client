using Bridge.Models.ClientServer.Gamification.Reward;

namespace UIManaging.Common.Rewards
{
    public interface IRewardView
    {
        void Show(IRewardModel reward, RewardState state);
        void Hide();
    }
}