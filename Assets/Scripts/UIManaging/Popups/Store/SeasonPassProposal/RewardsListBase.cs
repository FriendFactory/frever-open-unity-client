using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.AssetStore;
using Bridge.Models.ClientServer.Gamification;
using UnityEngine;

namespace UIManaging.Popups.Store.SeasonPassProposal
{
    internal abstract class RewardsListBase : MonoBehaviour
    {
        [SerializeField] protected RewardItemView _softCurrencyRewardPrefab;
        [SerializeField] protected RewardItemView _hardCurrencyRewardPrefab;
        [SerializeField] protected RewardItemView _wardrobeRewardPrefab;
        [SerializeField] protected RewardItemView _setLocationRewardPrefab;
        [SerializeField] protected RewardItemView _cameraFilterRewardPrefab;
        [SerializeField] protected RewardItemView _vfxRewardPrefab;
        [SerializeField] protected RewardItemView _voiceFilterPrefab;
        [SerializeField] protected RewardItemView _bodyAnimationPrefab;
        [SerializeField] protected int MaxItemToShow = 4;

        public bool Empty => ItemViews.Count == 0;
        
        protected readonly List<RewardItemView> ItemViews = new List<RewardItemView>();

        protected ICollection<SeasonRewardItemModel> CreateModels(ICollection<SeasonReward> seasonRewards)
        {
            var output = new List<SeasonRewardItemModel>();
            
            var softCurrencyRewards = seasonRewards.Where(x => x.SoftCurrency.HasValue).ToArray();
            if (softCurrencyRewards.Length > 0)
            {
                var softCurrencyModel = new SeasonRewardItemModel
                {
                    SoftCurrency = softCurrencyRewards.Sum(x => x.SoftCurrency.Value)
                };
                output.Add(softCurrencyModel);
            }
            
            var hardCurrencyRewards = seasonRewards.Where(x => x.HardCurrency.HasValue).ToArray();
            if (hardCurrencyRewards.Length > 0)
            {
                var hardCurrencyModel = new SeasonRewardItemModel
                {
                    HardCurrency = hardCurrencyRewards.Sum(x => x.HardCurrency.Value)
                };
                output.Add(hardCurrencyModel);
            }

            var otherRewards = seasonRewards.Where(x => !softCurrencyRewards.Contains(x) && !hardCurrencyRewards.Contains(x));
            foreach (var reward in otherRewards)
            {
                var rewardModel = new SeasonRewardItemModel
                {
                    Asset = reward.Asset
                };
                output.Add(rewardModel);
            }
            
            return output;
        }

        protected void DestroyAllViews()
        {
            foreach (var view in ItemViews)
            {
                view.transform.SetParent(null);
                Destroy(view.gameObject);
            }

            ItemViews.Clear();
        }

        protected RewardItemView GetPrefab(SeasonRewardItemModel seasonRewardItemModel)
        {
            if (seasonRewardItemModel.SoftCurrency.HasValue) return _softCurrencyRewardPrefab;
            if (seasonRewardItemModel.HardCurrency.HasValue) return _hardCurrencyRewardPrefab;
            var asset = seasonRewardItemModel.Asset;
            switch (asset.AssetType)
            {
                case AssetStoreAssetType.Wardrobe:
                    return _wardrobeRewardPrefab;
                case AssetStoreAssetType.SetLocation:
                    return _setLocationRewardPrefab;
                case AssetStoreAssetType.CameraFilter:
                    return _cameraFilterRewardPrefab;
                case AssetStoreAssetType.Vfx:
                    return _vfxRewardPrefab;
                case AssetStoreAssetType.VoiceFilter:
                    return _voiceFilterPrefab;
                case AssetStoreAssetType.BodyAnimation:
                    return _bodyAnimationPrefab;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}