using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Onboarding;
using Modules.QuestManaging;
using UIManaging.PopupSystem.Popups.Quests.Interfaces;

namespace UIManaging.PopupSystem.Popups.Quests.Models
{
    public class QuestPanelModel: IQuestPanelModel
    {
        private readonly IQuestManager _questManager;
        private readonly Dictionary<string, string> _redirectQuestsDictionary = new Dictionary<string, string>()
        {
            { QuestType.CREATE_VIDEO, QuestType.FEED_SWIPE }
        };
    
        public event Action QuestsUpdated;
        public event Action<OnboardingReward> RewardClaimed;
        public event Action<string> GoToQuestClicked;

        public IQuestGroupModel QuestGroupModel
        {
            get
            {
                if (_questGroupModel == null)
                {
                    var userQuestGroup = _questManager.UserQuestGroup;
                    _questGroupModel = new QuestGroupModel(userQuestGroup.QuestGroup, userQuestGroup.QuestActive, userQuestGroup.QuestProgress, GoToQuestTarget);
                }

                return _questGroupModel;
            }
        }
        
        public IQuestRewardModel QuestRewardModel 
        {
            get
            {
                if (_questRewardModel == null)
                {
                    var userQuestGroup = _questManager.UserQuestGroup;
                    var completed = userQuestGroup.QuestProgress.All(pair => pair.Value.CurrentProgress == pair.Value.ToComplete);
                    _questRewardModel = new QuestRewardModel(userQuestGroup.QuestGroup.Rewards?.FirstOrDefault(), completed, OnClaimReward);
                }

                return _questRewardModel;
            }
        }

        public string CurrentTitle => _questManager.UserQuestGroup.QuestGroup.Title;
        public int CurrentQuestGroupNumber => _questManager.CurrentQuestGroupNumber;
        public int TotalQuestGroupNumber => _questManager.TotalQuestGroupsAmount;

        private IQuestGroupModel _questGroupModel;
        private IQuestRewardModel _questRewardModel;

        public QuestPanelModel(IQuestManager questManager)
        {
            _questManager = questManager;
            _questManager.QuestDataUpdated += OnQuestDataUpdated;
            _questManager.RewardClaimed += OnRewardClaimed;
        }

        public void Cleanup()
        {
            _questManager.QuestDataUpdated -= OnQuestDataUpdated;
            _questManager.RewardClaimed -= OnRewardClaimed;
        }

        private void OnQuestDataUpdated()
        {
            _questGroupModel = null;
            _questRewardModel = null;
            
            QuestsUpdated?.Invoke();
        }

        private void OnClaimReward(long questGroupId)
        {
            _questManager.ClaimReward(questGroupId);
        }

        private void OnRewardClaimed(OnboardingReward reward)
        {
            _questGroupModel = null;
            _questRewardModel = null;
            
            RewardClaimed?.Invoke(reward);
        }

        private void GoToQuestTarget(string questType)
        {
            if (_redirectQuestsDictionary.TryGetValue(questType, out var redirectedType))
            {
                questType = redirectedType;
            }
            _questManager.GoToQuestTarget(questType);
            
            if (!_questManager.UserQuestGroup.QuestActive.ContainsKey(questType) ||
                _questManager.UserQuestGroup.QuestActive[questType])
            {
                GoToQuestClicked?.Invoke(questType);
            }
        }
    }
}