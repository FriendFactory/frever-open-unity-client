using System;
using Abstract;
using Bridge.Models.ClientServer.Gamification.Reward;
using Extensions;
using TMPro;
using UIManaging.Common.Rewards;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.SeasonPage
{
    internal sealed class SeasonRewardsLevelView : BaseContextDataView<SeasonRewardsLevelModel>
    {
        internal const float DEFAULT_CELL_SIZE = 420f;

        [SerializeField] private RectTransform _rectTransform;
        [Header("Background")]
        [SerializeField] private Image _background;
        [SerializeField] private Color _lockedColor;
        [SerializeField] private Color _unlockedColor;
        [Header("Levels")]
        [SerializeField] private RectTransform _levelLine;
        [SerializeField] private Image _levelBackground;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private Sprite _levelLocked;
        [SerializeField] private Sprite _levelUnlocked;
        [Header("Rewards")]
        [SerializeField] private CurrencyRewardView _freeCurrencyReward;
        [SerializeField] private CurrencyRewardView _premiumCurrencyReward;
        [SerializeField] private AssetRewardView _freeAssetRewardView;
        [SerializeField] private AssetRewardView _premiumAssetRewardView;
        
        private float _levelLineOffset;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            PrepareCellSize();
            PrepareBackground();
            PrepareLevelLine();
            PrepareFreeReward();
            PreparePremiumReward();    
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void PrepareCellSize()
        {
            var cellSize = DEFAULT_CELL_SIZE;
            _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, cellSize);
            _levelLineOffset = cellSize / 2f;
        }
        
        private void PrepareBackground()
        {
            _background.color = ContextData.CanBeClaimed ? _unlockedColor : _lockedColor;
        }

        private void PrepareLevelLine()
        {
            if (ContextData.IsFirst)
            {
                _levelLine.SetTop(_levelLineOffset);
                _levelLine.SetBottom(0);
            } 
            else if (ContextData.IsLast)
            {
                _levelLine.SetTop(0);
                _levelLine.SetBottom(_levelLineOffset);
            }
            else
            {
                _levelLine.SetTop(0);
                _levelLine.SetBottom(0);
            }
            
            _levelBackground.sprite = ContextData.CanBeClaimed ? _levelUnlocked : _levelLocked;
            _levelText.SetText(ContextData.Level.ToString());
        }

        private void PrepareFreeReward()
        {
            _freeCurrencyReward.Hide();
            _freeAssetRewardView.Hide();
            
            switch (ContextData.FreeReward.Type)
            {
                case RewardType.None:
                    return;
                case RewardType.SoftCurrency:
                case RewardType.HardCurrency:
                    PrepareReward(_freeCurrencyReward, ContextData.FreeReward);
                    return;
                case RewardType.Asset:
                    PrepareReward(_freeAssetRewardView, ContextData.FreeReward);
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void PreparePremiumReward()
        {
            _premiumCurrencyReward.Hide();
            _premiumAssetRewardView.Hide();

            switch (ContextData.PremiumReward.Type)
            {
                case RewardType.None:
                case RewardType.XP:
                    break;
                case RewardType.SoftCurrency:
                case RewardType.HardCurrency:
                    PrepareReward(_premiumCurrencyReward, ContextData.PremiumReward, !ContextData.IsPremium);
                    break;
                case RewardType.Asset:
                    PrepareReward(_premiumAssetRewardView, ContextData.PremiumReward, !ContextData.IsPremium);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void PrepareReward(IRewardView rewardView, SeasonRewardWrapper reward, bool isRewardLocked = false)
        {
            RewardState state;
            
            if (reward.IsClaimed)
            {
                state = RewardState.Claimed;
            }
            else if (isRewardLocked)
            {
                state = RewardState.Locked;
            }
            else if (ContextData.CanBeClaimed)
            {
                state = RewardState.Available;
            }
            else
            {
                state = RewardState.Obtainable;
            }

            rewardView.Show(reward.Reward, state);
        }
    }
}