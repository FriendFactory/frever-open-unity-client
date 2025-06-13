using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Gamification;
using Bridge.Models.ClientServer.Gamification.Reward;
using Common;
using Extensions;
using Modules.AssetsStoraging.Core;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.PopupSystem;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.SeasonPage
{
    internal sealed class SeasonRewardsContent : MonoBehaviour
    {
        [SerializeField] private GameObject _skeleton;
        [SerializeField] private SeasonRewardsListView _rewardsList;
        [SerializeField] private GameObject _premiumPassSkeleton;
        
        [Inject] private IDataFetcher _dataFetcher;
        [Inject] private LocalUserDataHolder _userData;
        [Inject] private PopupManagerHelper _popupManagerHelper;

        private bool _initialized;
        private Coroutine _skeletonRoutine;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnDestroy()
        {
            if (_skeletonRoutine != null)
            {
                CoroutineSource.Instance.StopCoroutine(_skeletonRoutine);
            }
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void ShowSkeleton()
        {
            _rewardsList.gameObject.SetActive(false);
            _skeleton.SetActive(true);
            _premiumPassSkeleton.SetActive(!_userData.HasPremiumPass);
            gameObject.SetActive(true);
        }

        public void Show()
        {
            if(!_initialized) Initialize();

            _rewardsList.SetActive(true);
            gameObject.SetActive(true);
            
            _rewardsList.RefreshActiveCards();
            
            _skeletonRoutine = CoroutineSource.Instance.ExecuteWithFrameDelay(() =>
            {
                _skeleton.SetActive(false);
                _rewardsList.UpdateScrollPosition();
            });
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        public void ReloadData()
        {
            Initialize();
            _rewardsList.ReloadData();
        }
        
        public void RefreshActiveData()
        {
            if (!_initialized)
            {
                return;
            }
            
            var currentLevel = _userData.LevelingProgress.Xp.CurrentLevel?.Level ?? 0;

            if (currentLevel > 0)
            {
                var divider = _rewardsList.ContextData.Items.FirstOrDefault(item => item is SeasonRewardsDividerModel);

                if (divider != null)
                {
                    _rewardsList.ContextData.Items.Remove(divider);
                }

                var itemCount = _rewardsList.ContextData.Items.Count(item => !(item is SeasonRepeatableBonusesRewardsLevelModel));

                if (currentLevel + 2 < itemCount)
                {
                    _rewardsList.ContextData.Items.Insert(currentLevel + 2, new SeasonRewardsDividerModel());
                }

                var count = 0;
                
                for (var i = 0; i < itemCount; i++)
                {
                    if (_rewardsList.ContextData.Items[i] is SeasonRewardsLevelModel seasonRewardsLevelModel)
                    {
                        seasonRewardsLevelModel.CanBeClaimed = count < currentLevel;
                        count++;
                    }
                }
            }
            
            _rewardsList.RefreshActiveCards();
        }

        public void SetRewardAsClaimed(long rewardId)
        {
            foreach (var itemModel in _rewardsList.ContextData.Items)
            {
                if (!(itemModel is SeasonRewardsLevelModel levelModel)) continue;
                
                if (levelModel.FreeReward.Reward?.Id == rewardId)
                {
                    levelModel.FreeReward.IsClaimed = true;
                    return;
                }
                    
                if (levelModel.PremiumReward.Reward?.Id == rewardId)
                {
                    levelModel.PremiumReward.IsClaimed = true;
                    return;
                }
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void Initialize()
        {
            var levels = _dataFetcher.CurrentSeason.Levels;
            var currentLevel = _userData.LevelingProgress.Xp.CurrentLevel?.Level ?? 0;
            var itemModels = GetItemModels(levels);
            var listModel = new SeasonRewardsListModel(itemModels, currentLevel);
            
            _rewardsList.Initialize(listModel);
            _initialized = true;
        }

        private List<SeasonRewardsItemModel> GetItemModels(IReadOnlyList<SeasonLevel> levels)
        {
            var publicLevels = levels.Where(level => level.LevelType == UserLevelType.Public && !level.Rewards.IsNullOrEmpty()).ToList();

            var models = new List<SeasonRewardsItemModel>();
            
            var currentLevel = _userData.LevelingProgress.Xp.CurrentLevel?.Level ?? 0;
            var isPremium = _userData.HasPremiumPass;
            if (isPremium)
            {
                var purchasedPassModel = GetPurchasedPassModel();
                models.Add(purchasedPassModel);
            }
            else
            {
                var headerModel = new SeasonRewardsPremiumPassModel
                {
                    OnSuccessfulPassPurchase = ReloadData 
                };
                models.Add(headerModel);
            }

            models.Add(new RewardTabsModel());
            
            var claimedRewards = _userData.LevelingProgress.RewardClaimed ?? Array.Empty<long>();
            
            var modelsCount = publicLevels.Count + 1;
            for (var i = 1; i < modelsCount; i++)
            {
                var level = publicLevels[i - 1];
                var isFirst = i == 1;
                var isLast = i == modelsCount - 1;
                var canBeClaimed = i <= currentLevel;

                var freeReward = GetFreeReward(level.Rewards, claimedRewards);
                var premiumReward = GetPremiumReward(level.Rewards, claimedRewards);

                var levelModel = new SeasonRewardsLevelModel(
                        level.Id, level.Level,
                        freeReward, premiumReward,
                        isFirst, isLast, isPremium,
                        canBeClaimed
                    );
                
                models.Add(levelModel);

                if (i != currentLevel || i == 0 || i >= modelsCount - 1) continue;
                
                var dividerModel = new SeasonRewardsDividerModel();
                models.Add(dividerModel);
            }
            
            var hiddenLevels = levels.Where(_ => _.LevelType == UserLevelType.Hidden)
                                     .OrderBy(_=>_.Level)
                                     .ToList();

            SeasonRepeatableBonusRewardsLevelModel nextLockedBonus = null;
            var bonusesCanBeClaimed = new List<SeasonRepeatableBonusRewardsLevelModel>();
            foreach (var hiddenLevel in hiddenLevels)
            {
                var canBeClaimed = hiddenLevel.Level <= currentLevel;
                var rewardWrapper = GetRepeatableBonusesReward(hiddenLevel.Rewards, claimedRewards);
                if (rewardWrapper == null || rewardWrapper.IsClaimed) continue;

                if (canBeClaimed)
                {
                    var bonusLevelModel = new SeasonRepeatableBonusRewardsLevelModel(
                        hiddenLevel.Id,
                        hiddenLevel.Level,
                        rewardWrapper);
                    bonusesCanBeClaimed.Add(bonusLevelModel);
                    continue;
                }

                nextLockedBonus = new SeasonRepeatableBonusRewardsLevelModel(
                    hiddenLevel.Id,
                    hiddenLevel.Level,
                    rewardWrapper);
                break;
            }

            if (bonusesCanBeClaimed.Count > 0 || nextLockedBonus != null)
            {
                var isBonusesLocked = hiddenLevels.All(_ => _.Level > currentLevel + 1);

                var bonusesLevelModel = new SeasonRepeatableBonusesRewardsLevelModel(bonusesCanBeClaimed, nextLockedBonus, isBonusesLocked);
                models.Add(bonusesLevelModel);
            }
            return models;
        }

        private SeasonRewardsPurchasedPremiumPassModel GetPurchasedPassModel()
        {
            void HidePurchasedPremiumPassPopup()
            {
                _popupManagerHelper.HidePremiumPassPurchaseSucceedPopup();
            }
            
            return new SeasonRewardsPurchasedPremiumPassModel
            {
                OnPurchasedPassClicked = () =>
                {
                    _popupManagerHelper.ShowPremiumPassPurchaseSucceedPopup(true,HidePurchasedPremiumPassPopup, HidePurchasedPremiumPassPopup);
                }
            };
        }

        private static SeasonRewardWrapper GetFreeReward(IEnumerable<SeasonReward> rewards, IEnumerable<long> claimedRewardIds)
        {
            var reward = rewards?.FirstOrDefault(item => !item.IsPremium);
            var type = reward?.GetRewardType() ?? RewardType.None;
            var isClaimed = claimedRewardIds.Any(id => id == reward?.Id);
            
            return new SeasonRewardWrapper(reward, type, isClaimed); 
        }
        
        private static SeasonRewardWrapper GetPremiumReward(IEnumerable<SeasonReward> rewards, IEnumerable<long> claimedRewardIds)
        {
            var reward = rewards?.FirstOrDefault(item => item.IsPremium);
            var type = reward?.GetRewardType() ?? RewardType.None;
            var isClaimed = claimedRewardIds.Any(id => id == reward?.Id);
            
            return new SeasonRewardWrapper(reward, type, isClaimed);
        }

        private static SeasonRewardWrapper GetRepeatableBonusesReward(IEnumerable<SeasonReward> rewards,
            IEnumerable<long> claimedRewardIds)
        {
            if (rewards == null) return null;

            var reward = rewards.FirstOrDefault(_ => _.SoftCurrency > 0);
            if (reward == null) return null;

            reward.IsPremium = false; //For the case when a premium reward for a repeated bonus is mistakenly received from the server.
            var type = RewardType.SoftCurrency;
            var isClaimed = claimedRewardIds.Any(id => id == reward.Id);
            return new SeasonRewardWrapper(reward, type, isClaimed);
        }
    }
}