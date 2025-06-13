using System;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Gamification.Reward;
using Bridge.Results;
using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UIManaging.Common.Rewards
{
    public class CurrencyRewardView : RewardView
    {
        [SerializeField] private Image _currencyIcon;
        [SerializeField] private TMP_Text _currencyText;

        [Header("Backgrounds")]
        [FormerlySerializedAs("_defaultBackground")]
        [SerializeField] private Sprite _defaultSoftBackground;
        [SerializeField] private Sprite _defaultHardBackground;

        [Header("Soft Currency")]
        [SerializeField] private Sprite _softCurrencyIcon;
        [SerializeField] private Vector2 _softCurrencySize;
        [SerializeField] private Vector2 _softCurrencyPosition;
        
        [Header("Hard Currency")]
        [SerializeField] private Sprite _hardCurrencyIcon;
        [SerializeField] private Vector2 _hardCurrencySize;
        [SerializeField] private Vector2 _hardCurrencyPosition;

        public event Action ClaimSuccess;

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        protected override void ShowAsAvailable()
        {
           base.ShowAsAvailable();
           UpdateCurrencyInfo();
        }
        
        protected override void ShowAsClaimed(bool updateThumbnail = true)
        {
            base.ShowAsClaimed(updateThumbnail);
            UpdateCurrencyInfo();
        }

        protected override void ShowAsObtainable()
        {
            base.ShowAsObtainable();
            UpdateCurrencyInfo();
        }

        protected override void ShowAsLocked()
        {
            base.ShowAsLocked();
            UpdateCurrencyInfo();
        }

        private void UpdateCurrencyInfo()
        {
            if (Reward.GetRewardType() == RewardType.SoftCurrency)
            {
                _thumbnailBackground.sprite = _defaultSoftBackground;
                _currencyIcon.sprite = _softCurrencyIcon;
                _currencyIcon.rectTransform.sizeDelta = _softCurrencySize;
                _currencyIcon.rectTransform.anchoredPosition = _softCurrencyPosition;
                _currencyText.text = Reward.SoftCurrency.ToString();
            }
            else
            {
                _thumbnailBackground.sprite = _defaultHardBackground;
                _currencyIcon.sprite = _hardCurrencyIcon;
                _currencyIcon.rectTransform.sizeDelta = _hardCurrencySize;
                _currencyIcon.rectTransform.anchoredPosition = _hardCurrencyPosition;
                _currencyText.text = Reward.HardCurrency.ToString();
            }
        }

        protected override void UpdateAlpha(float alpha)
        {
            _currencyIcon.color = _currencyIcon.color.SetAlpha(alpha);
            _currencyText.color = _currencyText.color.SetAlpha(alpha);
        }

        protected override Task<GetAssetResult> GetThumbnailRequest()
        {
            return null;
        }

        protected override void OnClaimButtonClicked()
        {
            _button.onClick.RemoveAllListeners();
            PageModel.OnCurrencyRewardClaimed(Reward.Id, OnClaimResult);
        }

        protected override void OnClaimSuccess()
        {
            base.OnClaimSuccess();
            
            ClaimSuccess?.Invoke();
        }
    }
}