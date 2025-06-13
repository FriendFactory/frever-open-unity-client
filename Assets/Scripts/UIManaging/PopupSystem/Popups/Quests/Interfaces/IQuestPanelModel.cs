using System;
using Bridge.Models.ClientServer.Onboarding;

namespace UIManaging.PopupSystem.Popups.Quests.Interfaces
{
    public interface IQuestPanelModel
    {
        event Action QuestsUpdated;
        event Action<OnboardingReward> RewardClaimed;
        
        IQuestGroupModel QuestGroupModel { get; }
        IQuestRewardModel QuestRewardModel { get; }
        string CurrentTitle { get; }
        int CurrentQuestGroupNumber { get; }
        int TotalQuestGroupNumber { get; }
    }
}