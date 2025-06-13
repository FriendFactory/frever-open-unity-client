using System;
using Bridge;
using Common;
using Extensions;
using Modules.AssetsStoraging.Core;
using TMPro;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.SeasonPage
{
    public class SeasonLevelPurchaseButton : MonoBehaviour
    {
        [SerializeField] private Button _purchaseLevelButton;
        [SerializeField] private Button _purchaseLevelConfirmButton;
        [SerializeField] private TextMeshProUGUI _purchaseLevelText;
        [SerializeField] private TextMeshProUGUI _purchaseLevelConfirmText;

        [Inject] private LocalUserDataHolder _dataHolder;
        [Inject] private IBridge _bridge;
        [Inject] private ISeasonProvider _seasonProvider;

        private int _levelAmount = 1;
        
        public int LevelAmount
        {
            get => _levelAmount;
            set
            {
                _levelAmount = value;
                UpdatePrice();
            }
        }

        public int Price => _seasonProvider.CurrentSeason.LevelHardCurrencyPrice;

        public event Action OnLevelPurchased;
        
        private void OnEnable()
        {
            _purchaseLevelConfirmButton.interactable = true;
            _purchaseLevelButton.onClick.AddListener(OnPurchaseLevel);
            _purchaseLevelConfirmButton.onClick.AddListener(OnPurchaseLevelConfirm);
            
            _purchaseLevelButton.SetActive(Price != 0);
            _purchaseLevelConfirmButton.SetActive(false);
            
            UpdatePrice();
        }

        private void OnDisable()
        {
            _purchaseLevelButton.onClick.RemoveListener(OnPurchaseLevel);
            _purchaseLevelConfirmButton.onClick.RemoveListener(OnPurchaseLevelConfirm);
        }
        
        private void OnPurchaseLevel()
        {
            _purchaseLevelButton.SetActive(false);
            _purchaseLevelConfirmButton.SetActive(true);
        }

        private async void OnPurchaseLevelConfirm()
        {
            _purchaseLevelConfirmButton.interactable = false;
            
            var result = await _bridge.PurchaseSeasonLevel(_dataHolder.CurrentLevel + LevelAmount);
            
            if (result.IsError)
            {
                Debug.LogError($"Failed to purchase season levels, reason: {result.ErrorMessage}");
            }
            
            _purchaseLevelConfirmButton.interactable = true;

            if (!result.IsSuccess)
            {
                return;
            }
            
            _purchaseLevelButton.SetActive(false);
            _purchaseLevelConfirmButton.SetActive(true);
            
            _dataHolder.RefreshUserInfo();
            
            OnLevelPurchased?.Invoke();
        }

        private void UpdatePrice()
        {
            var text = (LevelAmount * Price).ToString();

            _purchaseLevelText.text = text;
            _purchaseLevelConfirmText.text = text;
        }
    }
}