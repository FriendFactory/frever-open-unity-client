using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Onboarding;
using Bridge.Models.ClientServer.UserActivity;
using Common;
using Modules.Amplitude;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;

namespace Modules.QuestManaging
{
    [UsedImplicitly]
    internal sealed class QuestManager: IQuestManager
    {
        private const long SEASON_LIKES_QUEST_ID = 7;
        private const long SEASON_REWARDS_QUEST_ID = 8;
        
        [Inject] private LocalUserDataHolder _localUserDataHolder;
        [Inject] private IDataFetcher _dataFetcher;
        [Inject] private IRewardsBridge _bridge;
        [Inject] private IQuestRedirectionDictionary _questRedirectionDictionary;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private AmplitudeManager _amplitudeManager;

        public event Action QuestDataUpdated;
        public event Action<OnboardingReward> RewardClaimed;
        
        private UserQuestGroup _questGroup;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public int TotalQuestGroupsAmount => _dataFetcher.MetadataStartPack.OnboardingQuests.Count;

        public int CurrentQuestGroupNumber =>_dataFetcher.MetadataStartPack.OnboardingQuests
                                                         .TakeWhile(questGroup => _localUserDataHolder.LevelingProgress
                                                                       .OnboardingRewardClaimed?.Contains(questGroup.Id) ?? false)
                                                         .Count() + 1;

        public int QuestsRemainingInCurrentGroup => UserQuestGroup.QuestProgress.Count(pair => pair.Value.CurrentProgress < pair.Value.ToComplete);

        public UserQuestGroup UserQuestGroup
        {
            get
            {
                if (!_questGroup.Equals(default(UserQuestGroup)))
                {
                    return _questGroup;
                }

                var targetQuestGroup = _dataFetcher.MetadataStartPack.OnboardingQuests
                                                   .FirstOrDefault(questGroup => !_localUserDataHolder.LevelingProgress
                                                                      .OnboardingRewardClaimed
                                                                     ?.Contains(questGroup.Id) ?? true)
                                     ?? _dataFetcher.MetadataStartPack.OnboardingQuests.LastOrDefault();

                _questGroup = new UserQuestGroup
                {
                    QuestGroup = targetQuestGroup,
                    QuestActive = targetQuestGroup?.Quests.ToDictionary(quest => quest.QuestType, quest => CheckQuestAvailability(quest.Id)),
                    QuestProgress = targetQuestGroup?.Quests.ToDictionary(quest => quest.Id, 
                            quest => _localUserDataHolder.LevelingProgress.OnboardingQuestCompletion
                              ?.FirstOrDefault(questProgress => questProgress.OnboardingQuestId == quest.Id) 
                                  ?? new OnboardingQuestProgress { ToComplete = quest.QuestParameter ?? 1 }) 
                     ?? new Dictionary<long, OnboardingQuestProgress>()
                };

                return _questGroup;
            }
        }

        public bool IsUpdating { get; private set; }

        public bool IsComplete => _dataFetcher.MetadataStartPack.OnboardingQuests.All(
            questGroup => _localUserDataHolder.LevelingProgress
                                              .OnboardingRewardClaimed?.Contains(questGroup.Id) ?? false);

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public async Task UpdateQuestData()
        {
            IsUpdating = true;
            
            await _localUserDataHolder.RefreshUserInfoAsync();

            IsUpdating = false;
            _questGroup = default;
            QuestDataUpdated?.Invoke();
        }

        public void GoToQuestTarget(string questType)
        {
            if (UserQuestGroup.QuestActive.ContainsKey(questType) && !UserQuestGroup.QuestActive[questType])
            {
                _snackBarHelper.ShowInformationSnackBar("Come by tomorrow for the start of the next season!");
                return;
            }

            IQuestRedirection questRedirection = _questRedirectionDictionary.ContainsKey(questType)
                ? _questRedirectionDictionary[questType]
                : null;

            if (questRedirection == null)
            {
                Debug.LogError($"Failed to find quest redirection for quest {questType}");
                return;
            }
            
            questRedirection.Redirect();
        }
            
        public bool IsQuestCompleted(string questType)
        {
            return _localUserDataHolder.LevelingProgress.OnboardingQuestCompletion
            .Where(q => q.QuestType == questType)
            .All(q => q.IsCompleted);
        }

        public async void ShowQuestLikesSnackBar()
        {
            if (!_amplitudeManager.IsOnboardingQuestsFeatureEnabled())
            {
                return;
            }
            
            var previousOnboardingProgressData = _localUserDataHolder.LevelingProgress.OnboardingQuestCompletion
                    ?.FirstOrDefault(questProgress => questProgress.OnboardingQuestId == Constants.Quests.LIKE_VIDEOS_QUEST_ID);

            if (previousOnboardingProgressData != null && previousOnboardingProgressData.CurrentProgress >= previousOnboardingProgressData.ToComplete)
            {
                return;
            }
            
            await UpdateQuestData();

            _snackBarHelper.ShowOnboardingSeasonLikesSnackBar();
        }

        public async void ClaimReward(long questGroupId)
        {
            var targetQuestGroup =
                _dataFetcher.MetadataStartPack.OnboardingQuests.FirstOrDefault(questGroup => questGroup.Id == questGroupId);
            var targetQuestReward = targetQuestGroup?.Rewards.FirstOrDefault();

            if (targetQuestReward == null)
            {
                Debug.LogError($"Quest group not found or reward absent for quest group {questGroupId}");
                return;
            }

            var result = await _bridge.ClaimOnboardingReward(targetQuestReward.Id);

            if (result.IsError)
            {
                Debug.LogError($"Error receiving quest rewards, reason: {result.ErrorMessage}");
                return;
            }

            if (result.IsSuccess)
            {
                var reward = UserQuestGroup.QuestGroup.Rewards.FirstOrDefault();
                _questGroup = default;
                _localUserDataHolder.LevelingProgress.OnboardingRewardClaimed = _localUserDataHolder.LevelingProgress
                   .OnboardingRewardClaimed?.Concat(new[] { questGroupId  }).ToArray() ?? new[] { questGroupId  };
                RewardClaimed?.Invoke(reward);
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private bool CheckQuestAvailability(long questId)
        {
            switch (questId)
            {
                case SEASON_LIKES_QUEST_ID:
                case SEASON_REWARDS_QUEST_ID:
                    return _dataFetcher.CurrentSeason != null;
                default:
                    return true;
            }
        }
    }
}