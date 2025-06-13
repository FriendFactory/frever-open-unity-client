namespace UIManaging.Pages.Tasks.TaskRewardFlow
{
    public sealed class RewardModel
    {
        public int SoftCurrencyReward { get; }
        
        public RewardModel(int softCurrencyReward)
        {
            SoftCurrencyReward = softCurrencyReward;
        }
    }
}