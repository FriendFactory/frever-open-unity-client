using System.Collections.Generic;
using UIManaging.Pages.Tasks.RewardPopUp;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.SeasonPage.UnclaimedRewards
{
    internal class SeasonUnclaimedRewardsPopup : BasePopup<SeasonUnclaimedRewardsPopupConfiguration>
    {
        [SerializeField] private Button _claimButton;
        [SerializeField] private GridLayoutGroup _gridLayout;
        [SerializeField] private VerticalLayoutGroup _gridVerticalLayout;
        [SerializeField] private ScrollRect _gridScrollRect;
        [SerializeField] private LayoutElement _gridLayoutElement;
        [SerializeField] private RectTransform _itemsParent;
        [SerializeField] private AssetRewardItem _rewardElementPrefab;
        
        [SerializeField] private TaskCompletedPopupRewardElement _softCurrencyReward;
        [SerializeField] private TaskCompletedPopupRewardElement _hardCurrencyReward;
        
        [SerializeField] private int _scrollableRowsThreshold = 20;

        private readonly List<AssetRewardItem> _assetElements = new List<AssetRewardItem>();

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _claimButton.onClick.AddListener(OnClaimButton);
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void Hide()
        {
            Hide(Configs.Rewards);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnConfigure(SeasonUnclaimedRewardsPopupConfiguration configuration)
        {
            PopulateItems();
            SetupScrollRect();
        }

        protected override void OnHidden()
        {
            _softCurrencyReward.gameObject.SetActive(false);
            _hardCurrencyReward.gameObject.SetActive(false);

            foreach (var item in _assetElements)
            {
                Destroy(item.gameObject);
            }
            
            _assetElements.Clear();
            
            base.OnHidden();
        }
        
        //---------------------------------------------------------------------
        // Private
        //---------------------------------------------------------------------

        private void OnClaimButton()
        {
            Hide();
        }
        
        private void PopulateItems()
        {
            _softCurrencyReward.gameObject.SetActive(false);
            _hardCurrencyReward.gameObject.SetActive(false);

            if(Configs.Rewards.SoftCurrency > 0)
            {
                _softCurrencyReward.gameObject.SetActive(true);
                _softCurrencyReward.Show(Configs.Rewards.SoftCurrency.ToString());
            }
            if (Configs.Rewards.HardCurrency > 0)
            {
                _hardCurrencyReward.gameObject.SetActive(true);
                _hardCurrencyReward.Show(Configs.Rewards.HardCurrency.ToString());
            }

            if (Configs.Rewards?.Assets is null) return;
            
            foreach (var reward in Configs.Rewards.Assets)
            {
                var item = Instantiate(_rewardElementPrefab, _itemsParent);
                _assetElements.Add(item);
                item.Initialize(reward.Asset);
            }
        }

        private void SetupScrollRect()
        {
            var isScrollable = Configs.Rewards.RewardCount / _gridLayout.constraintCount > _scrollableRowsThreshold;

            _gridScrollRect.enabled = isScrollable;
            _gridVerticalLayout.enabled = !isScrollable;

            _gridLayoutElement.preferredHeight = isScrollable 
                ? (_gridLayout.cellSize.y + _gridLayout.spacing.y) * (_scrollableRowsThreshold + 0.5f)
                : 0;
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(_gridScrollRect.transform as RectTransform);
        }

    }
}