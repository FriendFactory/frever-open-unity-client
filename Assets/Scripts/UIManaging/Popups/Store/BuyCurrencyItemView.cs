using System;
using Abstract;
using Modules.InAppPurchasing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Popups.Store
{
    public abstract class BuyCurrencyItemView<T> : BaseContextDataView<T>, IProductItemView where T: PurchasableProduct
    {
        private const string CURRENCY_ICON = "<sprite index=0>";
        
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TextMeshProUGUI _price;
        [SerializeField] private Button _buyButton;
        [SerializeField] private GameObject _loadingUi;
        [SerializeField] private string _identifier;

        [Inject] protected IIAPManager IAPManager;
        
        public string ItemIdentifier => _identifier;
        public PurchasableProduct Product => ContextData;
        public event Action<PurchasableProduct> Selected;
        public event Action<PurchasableProduct> NotEnoughCurrencyForPurchase;

        public void Initialize(PurchasableProduct product)
        {
            base.Initialize(product as T);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        protected override void OnInitialized()
        {
            _name.text = $"{CURRENCY_ICON}{ContextData.Amount}";

            var pricePrefix = ContextData.Type == ProductType.SoftCurrency ? CURRENCY_ICON : string.Empty;
            _price.text = $"{pricePrefix}{ContextData.PriceText}";
            
            _buyButton.onClick.AddListener(OnBuyClicked);
            IAPManager.PurchaseConfirmed += OnPurchaseSuccess;
            IAPManager.PurchaseFailed += OnPurchaseFailed;
        }

        protected override void BeforeCleanup()
        {
            _buyButton.onClick.RemoveListener(OnBuyClicked);
            IAPManager.PurchaseConfirmed -= OnPurchaseSuccess;
            IAPManager.PurchaseFailed -= OnPurchaseFailed;
            base.BeforeCleanup();
        }

        protected virtual void OnBuyClicked()
        {
            Selected?.Invoke(ContextData);
            ShowLoadingUI(true);
        }
        
        protected void OnNotEnoughCurrencyForPurchase()
        {
            NotEnoughCurrencyForPurchase?.Invoke(ContextData);
        }

        protected void ShowLoadingUI(bool show)
        {
            _loadingUi.SetActive(show);
        }

        private void OnPurchaseSuccess(PurchasableProduct product)
        {
            ShowLoadingUI(false);
        }
        
        private void OnPurchaseFailed()
        {
            ShowLoadingUI(false);
        }
    }
}