using Extensions;
using Modules.Amplitude;
using Modules.InAppPurchasing;
using QFSW.QC;
using TMPro;
using UIManaging.Localization;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Popups.Store.SeasonPassProposal.Popup
{
    internal sealed class BuyPremiumPassButton: MonoBehaviour
    {
        
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private GameObject _loadingIcon;
        
        [Inject] private IIAPManager _iapManager;
        [Inject] private AmplitudeManager _amplitudeManager;
        [Inject] private SeasonPageLocalization _localization;
        [Inject] private LocalUserDataHolder _localUserData;

        private SeasonPassProduct Product => _iapManager.GetSeasonPassProduct();
        private bool IsFree => Product.IsFree;
        private bool Interactable
        {
            get => _button.interactable;
            set => _button.interactable = value;
        }

        private void Awake()
        {
            _button.onClick.AddListener(OnClick);
        }

        public void Setup()
        {
            _priceText.text = IsFree ? _localization.SeasonRewardClaimButtonText : Product.PriceText;
            Interactable = true;
        }
        
        private void OnClick()
        {
            Interactable = false;
            _priceText.SetActive(false);
            _loadingIcon.SetActive(true);
            if (IsFree)
            {
                _iapManager.ClaimFreeSeasonPass();
            }
            else
            {
                SubscribeToPurchasingEvents();
                _iapManager.BuyProduct(Product);
            }
        }

        private void SubscribeToPurchasingEvents()
        {
            UnsubscribeFromPurchasingEvents();
            _iapManager.PurchaseConfirmed += OnPurchaseConfirmed;
            _iapManager.PurchaseFailed += OnPurchaseFailed;
        }
        
        private void UnsubscribeFromPurchasingEvents()
        {
            _iapManager.PurchaseConfirmed -= OnPurchaseConfirmed;
            _iapManager.PurchaseFailed -= OnPurchaseFailed;
        }
        
        private void OnPurchaseConfirmed(PurchasableProduct product)
        {
            UnsubscribeFromPurchasingEvents();
            HideLoading();
            Interactable = true;
        }

        private void OnPurchaseFailed()
        {
            UnsubscribeFromPurchasingEvents();
            HideLoading();
            Interactable = true;
        }

        private void HideLoading()
        {
            _loadingIcon.SetActive(false);
            _priceText.SetActive(true);
        }
    }
}