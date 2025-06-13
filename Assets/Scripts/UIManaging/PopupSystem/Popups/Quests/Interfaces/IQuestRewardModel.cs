using System;
using UIManaging.PopupSystem.Popups.Quests.Models;

namespace UIManaging.PopupSystem.Popups.Quests.Interfaces
{
    public interface IQuestRewardModel
    {
        long Id { get; }
        string Title { get; }
        int Value { get; }
        QuestRewardType Type { get; }
        bool Completed { get; }
        Action RewardClaimAction { get; }
    }
}