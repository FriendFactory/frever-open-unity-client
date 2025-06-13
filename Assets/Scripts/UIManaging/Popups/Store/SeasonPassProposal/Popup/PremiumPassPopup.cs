using System.Threading;
using Bridge.Models.ClientServer.Gamification;
using Extensions;
using Extensions.DateTime;
using Modules.AssetsStoraging.Core;
using Modules.InAppPurchasing;
using TMPro;
using UIManaging.Localization;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Popups.Store.SeasonPassProposal.Popup
{
    internal sealed class PremiumPassPopup : BasePopup<PremiumPassPopupConfiguration>
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private RewardsList _rewardsList;
        [SerializeField] private PremiumPassBackground _premiumPassBackground;
        [SerializeField] private BuyPremiumPassButton _buyButton;
        [SerializeField] private TextMeshProUGUI _timeLeftText;
        [SerializeField] private GameObject _loadingOverlay;
        
        [Inject] private ISeasonProvider _seasonProvider;
        [Inject] private IIAPManager _iapManager;
        [Inject] private LocalUserDataHolder _userData;
        [Inject] private StorePageLocalization _localization;

        private CancellationTokenSource _cancellationTokenSource;

        private SeasonReward[] PremiumRewards => _seasonProvider.CurrentSeason.GetPremiumLevelRewards();

        private void Awake()
        {
            _closeButton.onClick.AddListener(Hide);
        }

        protected override void OnConfigure(PremiumPassPopupConfiguration configuration)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _premiumPassBackground.Setup();
            _buyButton.Setup();
            _timeLeftText.text = string.Format(_localization.PremiumPassTimeLeft, _seasonProvider.CurrentSeason.EndDate.GetFormattedTimeLeft());
        }

        public override void Show()
        {
            base.Show();
            var alreadyInitializedRewards = !_rewardsList.Empty;
            if (!alreadyInitializedRewards)
            {
                _rewardsList.Setup(PremiumRewards);
            }
           
            _iapManager.PurchaseStarted += OnPurchasingStarted;
            _iapManager.PurchaseFailed += OnPurchasingFailed;
            _iapManager.PurchaseConfirmed += OnPurchasingConfirmed;
        }

        public override void Hide()
        {
            _cancellationTokenSource?.CancelAndDispose();
            _cancellationTokenSource = null;
            
            _iapManager.PurchaseStarted -= OnPurchasingStarted;
            _iapManager.PurchaseFailed -= OnPurchasingFailed;
            _iapManager.PurchaseConfirmed -= OnPurchasingConfirmed;
           
            base.Hide();
        }

        private void OnPurchasingStarted()
        {
            _loadingOverlay.SetActive(true);
        }

        private void OnPurchasingFailed()
        {
            _loadingOverlay.SetActive(false);
            Hide();
        }

        private async void OnPurchasingConfirmed(PurchasableProduct product)
        {
            await _userData.RefreshUserInfoAsync();
            Configs.OnSuccessfulPassPurchase?.Invoke();
            _loadingOverlay.SetActive(false);
        }
    }
}
