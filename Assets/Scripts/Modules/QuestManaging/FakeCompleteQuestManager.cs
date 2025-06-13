using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Onboarding;
using Bridge.Models.ClientServer.UserActivity;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using UIManaging.Pages.Common.UsersManagement;
using Zenject;

namespace Modules.QuestManaging
{
    [UsedImplicitly]
    internal sealed class FakeCompleteQuestManager: IQuestManager
    {
        [Inject] private LocalUserDataHolder _localUserDataHolder;
        [Inject] private IDataFetcher _dataFetcher;

        public event Action QuestDataUpdated;
        public event Action<OnboardingReward> RewardClaimed;
        
        private UserQuestGroup _questGroup;
        private int _claimedCount;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public int TotalQuestGroupsAmount => _dataFetcher.MetadataStartPack.OnboardingQuests.Count;

        public int CurrentQuestGroupNumber => _claimedCount;

        public int QuestsRemainingInCurrentGroup => 0;

        public UserQuestGroup UserQuestGroup
        {
            get
            {
                if (!_questGroup.Equals(default(UserQuestGroup)))
                {
                    return _questGroup;
                }

                var targetQuestGroup = _dataFetcher.MetadataStartPack.OnboardingQuests.Skip(_claimedCount).FirstOrDefault() 
                                    ?? _dataFetcher.MetadataStartPack.OnboardingQuests.LastOrDefault();

                _questGroup = new UserQuestGroup
                {
                    QuestGroup = targetQuestGroup,
                    QuestActive = targetQuestGroup?.Quests.ToDictionary(quest => quest.QuestType, quest => true),
                    QuestProgress = targetQuestGroup?.Quests.ToDictionary(quest => quest.Id, quest => new OnboardingQuestProgress
                    {
                        ToComplete = quest.QuestParameter ?? 1,
                        CurrentProgress = quest.QuestParameter ?? 1
                    }) ?? new Dictionary<long, OnboardingQuestProgress>()
                };

                return _questGroup;
            }
        }

        public bool IsUpdating { get; private set; }
        public bool IsComplete => _claimedCount >= TotalQuestGroupsAmount;

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
            
        }

        public void ClaimReward(long questGroupId)
        {
            _claimedCount++;
            
            _questGroup = default;
            RewardClaimed?.Invoke(UserQuestGroup.QuestGroup.Rewards.FirstOrDefault());
        }

        public bool IsQuestCompleted(string questType)
        {
            throw new NotImplementedException();
        }

        public void ShowQuestLikesSnackBar()
        {
            
        }
    }
}