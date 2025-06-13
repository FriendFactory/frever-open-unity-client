namespace UIManaging.Pages.Tasks.TaskRewardFlow
{
    public class RewardFlowResult
    {
        public readonly bool LeveledUp;
        public readonly int OldLevel;
        public readonly int NewLevel;
        
        public RewardFlowResult(int oldLevel, int newLevel)
        {
            OldLevel = oldLevel;
            NewLevel = newLevel;
            LeveledUp = NewLevel > OldLevel;
        }
    }
}