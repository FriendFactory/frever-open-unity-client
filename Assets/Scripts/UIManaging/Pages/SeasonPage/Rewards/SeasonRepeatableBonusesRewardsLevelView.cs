using System.Collections.Generic;
using System.Linq;
using Abstract;
using I2.Loc;
using TMPro;
using UIManaging.Common;
using UIManaging.Common.Rewards;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UIManaging.Pages.SeasonPage
{
    internal sealed class SeasonRepeatableBonusesRewardsLevelView : BaseContextDataView<SeasonRepeatableBonusesRewardsLevelModel>
    {
        [Header("Level")]
        [SerializeField] private TextMeshProUGUI _unlockingDescriptionText;
        [SerializeField] private Transform _taskDescriptionContainer;
        [SerializeField] private TextMeshProUGUI _taskDescriptionText;
        [SerializeField] private Toggle _taskToggle;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Transform _dividerContainer;

        [Header("Background color")] 
        [SerializeField] private Color _lockedColor;
        [SerializeField] private Color _unlockedColor;

        [Header("Rewards")]
        [SerializeField] private CurrencyRewardView _freeCurrencyReward;
        
        [Header("Localization")]
        [SerializeField] private LocalizedString _levelUnlockRequirementFormat;

        private List<SeasonRepeatableBonusRewardsLevelModel> _canBeClaimed;
        private SeasonRepeatableBonusRewardsLevelModel _nextLockedBonus;
        private bool _isBonusesLocked;

        public event UnityAction<SeasonRepeatableBonusesRewardsLevelModel> OnClaimedAllRepeatableBonusesRewards;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            _canBeClaimed = ContextData.BonusesCanBeClaimed
                                         .Where(_ => !_.Reward.IsClaimed)
                                         .OrderBy(_ => _.Level)
                                         .ToList();
            
            _nextLockedBonus = ContextData.NextLockedBonus;
            _isBonusesLocked = ContextData.IsBonusesLocked;
            SubscribeClaiming(_freeCurrencyReward);
            UpdateUi();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void UpdateUi()
        {
            if (_isBonusesLocked)
            {
                ShowAsLocked();
                return;
            }

            if (_canBeClaimed.Count > 0)
            {
                var model = _canBeClaimed[0];
                ShowAsCanBeClaimed(model);
                return;
            }

            if (_nextLockedBonus != null)
            {
                ShowAsCanNotBeClaimed(_nextLockedBonus);
            }
            else
            {
                OnClaimedAllRepeatableBonusesRewards?.Invoke(ContextData);
            }
        }

        private void ShowAsLocked()
        {
            ShowDescription(true);

            _freeCurrencyReward.Show(_nextLockedBonus.Reward.Reward, RewardState.Locked);
        }

        private void ShowAsCanBeClaimed(SeasonRepeatableBonusRewardsLevelModel bonusModel)
        {
            ShowDescription(false, bonusModel.Level);
            _taskToggle.isOn = true;

            _freeCurrencyReward.Show(bonusModel.Reward.Reward, RewardState.Available);
        }

        private void ShowAsCanNotBeClaimed(SeasonRepeatableBonusRewardsLevelModel bonusModel)
        {
            ShowDescription(false, bonusModel.Level);
            _taskToggle.isOn = false;

            _freeCurrencyReward.Show(bonusModel.Reward.Reward, RewardState.Obtainable);
        }

        private void ShowDescription(bool isLockedBonuses, int level = 0)
        {
            _unlockingDescriptionText.gameObject.SetActive(isLockedBonuses);

            _dividerContainer.gameObject.SetActive(!isLockedBonuses);
            _taskDescriptionContainer.gameObject.SetActive(!isLockedBonuses);
            _taskDescriptionText.gameObject.SetActive(!isLockedBonuses);
            _taskDescriptionText.text = string.Format(_levelUnlockRequirementFormat, level);
            _backgroundImage.color = isLockedBonuses ? _lockedColor : _unlockedColor;
        }

        private void SubscribeClaiming(CurrencyRewardView currencyRewardView)
        {
            currencyRewardView.ClaimSuccess -= ClaimSuccess;
            currencyRewardView.ClaimSuccess += ClaimSuccess;
        }

        private void ClaimSuccess()
        {
            if (_canBeClaimed.Count > 0)
            {
                _canBeClaimed[0].Reward.IsClaimed = true;
                _canBeClaimed.RemoveAt(0);
            }
            UpdateUi();
        }
    }
}