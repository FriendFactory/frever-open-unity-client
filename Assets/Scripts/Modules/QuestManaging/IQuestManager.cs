using System;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Onboarding;

namespace Modules.QuestManaging
{
    public interface IQuestManager
    {
        event Action QuestDataUpdated;
        event Action<OnboardingReward> RewardClaimed;
        
        int TotalQuestGroupsAmount { get; }
        int CurrentQuestGroupNumber { get; }
        int QuestsRemainingInCurrentGroup { get; }
        UserQuestGroup UserQuestGroup { get; }
        bool IsUpdating { get; }
        bool IsComplete { get; }

        Task UpdateQuestData();
        void GoToQuestTarget(string questType);
        void ClaimReward(long questGroupId);
        bool IsQuestCompleted(string questType);
        void ShowQuestLikesSnackBar();
    }
}