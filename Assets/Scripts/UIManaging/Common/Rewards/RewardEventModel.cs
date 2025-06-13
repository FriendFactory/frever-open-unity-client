using System;
using JetBrains.Annotations;
using UnityEngine;

namespace UIManaging.Common.Rewards
{
    [UsedImplicitly]
    public sealed class RewardEventModel
    {
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<long, Action<bool>> CurrencyRewardClaimed;
        public event Action<long, Sprite, Action<bool>> AssetRewardClaimed;
        public event Action<long, Sprite, Action<bool>>  LootboxRewardClaimed;
        public event Action<long> LockedRewardClicked;
        public event Action<long> PreviewRequested;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void OnCurrencyRewardClaimed(long rewardId, Action<bool> callback)
        {
            CurrencyRewardClaimed?.Invoke(rewardId, callback);
        }

        public void OnAssetRewardClaimed(long rewardId, Sprite thumbnail, Action<bool> callback)
        {
            AssetRewardClaimed?.Invoke(rewardId, thumbnail, callback);
        }
        
        public void OnLockedRewardClicked(long rewardId)
        {
            LockedRewardClicked?.Invoke(rewardId);
        }
        
        public void OnPreviewRequested(long rewardId)
        {
            PreviewRequested?.Invoke(rewardId);
        }

        public void OnLootboxRewardClaimed(long lootBoxId, Sprite thumbnail, Action<bool> callback)
        {
            LootboxRewardClaimed?.Invoke(lootBoxId, thumbnail, callback);
        }
    }
}