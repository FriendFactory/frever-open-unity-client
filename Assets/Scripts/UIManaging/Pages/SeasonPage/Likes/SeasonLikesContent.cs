using System;
using System.Collections.Generic;
using System.Linq;
using Bridge;
using Bridge.Models.ClientServer.Gamification;
using Extensions;
using Modules.AssetsStoraging.Core;
using Navigation.Core;
using UIManaging.Pages.Common.Seasons;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Tasks.TaskRewardFlow;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.SeasonPage.Likes
{
    public class SeasonLikesContent : MonoBehaviour
    {
        [SerializeField] private GameObject _skeleton;
        [SerializeField] private SeasonLikesListView _likesList;
        [SerializeField] private SeasonRewardFlowManager _seasonRewardFlowManager;
        [SerializeField] private SeasonLevelInfoView _seasonLevelInfoView;

        [Inject] private IDataFetcher _dataFetcher;
        [Inject] private LocalUserDataHolder _userData;
        [Inject] private IBridge _bridge;
        [Inject] private PageManager _pageManager;

        private bool _isInitialized;

        public event Action SwitchToRewardTabRequested;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnDestroy()
        {
            CleanUp();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void ShowSkeleton()
        {
            _skeleton.SetActive(true);
            _likesList.SetActive(false);
            gameObject.SetActive(true);
        }

        public void Show(long? startQuestId = null)
        {
            _skeleton.SetActive(false);
            _likesList.SetActive(true);
            gameObject.SetActive(true);

            if (_isInitialized) return;

            Initialize(startQuestId);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void Initialize(long? startQuestId = null)
        {
            var quests = _dataFetcher.CurrentSeason.Quests.OrderBy(quest => quest.Value).ToArray();
            var itemModels = GetItemModels(quests);
            var listModel = new SeasonLikesListModel(itemModels, startQuestId);

            _likesList.Initialize(listModel);
            _likesList.UpdateScrollPosition();
            _isInitialized = true;
        }

        private void CleanUp()
        {
            if (_likesList.ContextData == null)
            {
                return;
            }

            var seasonLikesQuestModels = _likesList.ContextData.Items
                                                   .Where(item => item is SeasonLikesQuestModel)
                                                   .Cast<SeasonLikesQuestModel>();

            foreach (var questModel in seasonLikesQuestModels)
            {
                questModel.OnClaimReward -= OnClaimReward;
            }

            _likesList.CleanUp();
        }

        private SeasonLikesItemModel[] GetItemModels(IReadOnlyList<SeasonQuest> quests)
        {
            var modelsCount = quests.Count + 1;
            var models = new SeasonLikesItemModel[modelsCount];

            models[0] = new SeasonLikesHeaderModel(_userData.LevelingProgress.LikesReceivedThisSeason);

            for (var i = 1; i < modelsCount; i++)
            {
                var quest = quests[i - 1];
                var id = quest.Id;
                var reward = quest.Rewards?.Length > 0 ? quest.Rewards[0] : null;
                var claimed = reward != null &&
                              (_userData.LevelingProgress.RewardClaimed?.Contains(reward.Id) ?? false);
                var currentUserLikes = _userData.LevelingProgress.LikesReceivedThisSeason;
                var currentQuestLikes = quest.Value;
                var previousQuestLikes = i < 2 ? null : (int?)quests[i - 2].Value;
                var nextQuestLikes = i >= modelsCount - 1 ? null : (int?)quests[i].Value;

                var model = new SeasonLikesQuestModel(id, claimed, reward, currentUserLikes, currentQuestLikes,
                                                      previousQuestLikes, nextQuestLikes, quest.Title);

                model.OnClaimReward += OnClaimReward;

                models[i] = model;
            }

            return models;
        }

        private async void OnClaimReward(long questId)
        {
            _seasonRewardFlowManager.Initialize();

            var result = await _bridge.ClaimSeasonQuestReward(questId);
            
            if (result.IsError)
            {
                Debug.LogError($"Failed to claim season reward, reason: {result.ErrorMessage}");
                return;
            }
            
            var targetItem = _likesList.ContextData.Items
                                       .Where(item => item is SeasonLikesQuestModel)
                                       .Cast<SeasonLikesQuestModel>()
                                       .FirstOrDefault(item => item.Reward != null 
                                                            && result.SeasonRewardIds.Contains(item.Reward.Id));

            if (result.IsSuccess)
            {
                targetItem?.ConfirmClaim();
                _seasonRewardFlowManager.Run(result.Xp, 0, 0);
                _seasonRewardFlowManager.FlowCompleted = OnClaimFlowDone;
            }
        }


        private void OnClaimFlowDone(bool transition)
        {
            _seasonRewardFlowManager.FlowCompleted -= OnClaimFlowDone;
            _seasonLevelInfoView.Initialize(new SeasonLevelInfoStaticModel(_userData));

            if (transition)
            {
                SwitchToRewardTabRequested?.Invoke();
            }
        }
    }
}