using System;
using Abstract;
using Bridge.Models.ClientServer.Gamification.Reward;
using Extensions;
using UIManaging.Common.Rewards;
using UIManaging.Pages.Crews.TrophyHunt.Models;
using UnityEngine;

namespace UIManaging.Pages.Crews.TrophyHunt
{
    public class TrophyHuntRewardListItem : BaseContextDataView<CrewRewardWrapper>
    {
        [SerializeField] private CurrencyRewardView _currencyReward;
        [SerializeField] private AssetRewardView _assetReward;
        [SerializeField] private LootboxRewardView _lootboxRewardView;
        [SerializeField] private GameObject _unlockedBackground;
        
        protected override void OnInitialized()
        {
            var rewardType = ContextData.Reward.GetRewardType();

            switch (rewardType)
            {
                case RewardType.HardCurrency:
                case RewardType.SoftCurrency:
                    InitializeCurrencyView();
                    break;
                case RewardType.Asset:
                    InitializeAssetView();
                    break;
                case RewardType.Lootbox:
                    InitializeLootboxView();
                    break;
                default: throw new ArgumentOutOfRangeException("RewardType","Unexpected reward type");
            }
            
            if(_unlockedBackground) 
            {
                _unlockedBackground.SetActive(ContextData.State == RewardState.Available 
                                            || ContextData.State == RewardState.Claimed);
            }
        }

        private void InitializeAssetView()
        {
            _assetReward.SetActive(true);
            _currencyReward.SetActive(false);
            _lootboxRewardView.SetActive(false);
            
            _assetReward.Show(ContextData.Reward, ContextData.State);
        }

        private void InitializeCurrencyView()
        {
            _currencyReward.SetActive(true);
            _assetReward.SetActive(false);
            _lootboxRewardView.SetActive(false);
            
            _currencyReward.Show(ContextData.Reward, ContextData.State);
        }
        
        private void InitializeLootboxView()
        {
            _lootboxRewardView.SetActive(true);
            _currencyReward.SetActive(false);
            _assetReward.SetActive(false);

            _lootboxRewardView.Show(ContextData.Reward,  ContextData.State);
        }
    }
}