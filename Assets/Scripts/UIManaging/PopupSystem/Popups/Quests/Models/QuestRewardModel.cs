using System;
using Bridge.Models.ClientServer.Onboarding;
using UIManaging.PopupSystem.Popups.Quests.Interfaces;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.Quests.Models
{
    public class QuestRewardModel: IQuestRewardModel
    {
        public long Id { get; }
        public string Title { get; }
        public int Value { get; }
        public QuestRewardType Type { get; }
        public bool Completed { get; }
        public Action RewardClaimAction { get; }

        public QuestRewardModel(OnboardingReward reward, bool completed, Action<long> rewardClaimAction)
        {
            Id = reward.Id;
            Title = reward.Title;
            Value = reward.SoftCurrency ?? reward.HardCurrency ?? 0;

            if (reward.SoftCurrency.HasValue == reward.HardCurrency.HasValue || (reward.Asset?.Id ?? 0) != 0 ||
                reward.Xp.HasValue)
            {
                Debug.LogError($"Unsupported quest reward type for reward {Id}");
                Type = QuestRewardType.Unsupported;
            }
            else
            {
                Type = reward.SoftCurrency.HasValue ? QuestRewardType.SoftCurrency : QuestRewardType.HardCurrency;
            }

            Completed = completed;
            RewardClaimAction = () => rewardClaimAction?.Invoke(Id);
        }
    }
}