using System;
using System.Collections.Generic;
using System.Linq;
using Abstract;
using Extensions;
using Modules.InAppPurchasing;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Popups.Store.SeasonPassProposal;
using UnityEngine;
using Zenject;

namespace UIManaging.Popups.Store
{
    internal sealed class StoreProductsListView : BaseContextDataView<StoreProductsListModel>
    {
        [SerializeField] private Transform _container;
        [Header("Prefabs")]
        [SerializeField] private SeasonPassItemView _premiumPass;
        [SerializeField] private PurchasedPassView _premiumPassPurchased;
        [SerializeField] private BuyGemsItemView[] _gemCurrencyItemViews;
        [SerializeField] private BuyCoinsItemView[] _coinCurrencyItemViews;
        [SerializeField] private Transform _gridBottomSpacer;
        
        [Inject] private LocalUserDataHolder _userDataHolder;
        private PurchasedPassView _premiumPassPurchasedInstance;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        private readonly List<IProductItemView> _productItems = new List<IProductItemView>();
        private bool PremiumPassPurchased => _userDataHolder.HasPremiumPass;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<PurchasableProduct> ProductSelected;
        public event Action<PurchasableProduct> NotEnoughFundsForPurchase;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            HideAllProducts();
            
            if (PremiumPassPurchased)
            {
                ShowPremiumPassAlreadyPurchased();
            }

            var itemsToShow = PremiumPassPurchased 
                ? ContextData.Products.Where(x => x.Type != ProductType.SeasonPass) 
                : ContextData.Products;

            ShowProducts(itemsToShow);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void ShowPremiumPassAlreadyPurchased()
        {
            if (_premiumPassPurchasedInstance == null)
            {
                _premiumPassPurchasedInstance = Instantiate(_premiumPassPurchased, _container);
            }
            _premiumPassPurchasedInstance.SetActive(true);
            _premiumPassPurchasedInstance.transform.SetSiblingIndex(0);
        }
        
        private void ShowProducts(IEnumerable<PurchasableProduct> products)
        {
            foreach (var product in products)
            {
                var view = GetView(product);
                view.Initialize(product);
                view.Show();
            }
        }

        private IProductItemView GetView(PurchasableProduct product)
        {
            return _productItems.FirstOrDefault(x => x.Product.ReferenceId == product.ReferenceId) ?? CreateView(product);
        }

        private IProductItemView CreateView(PurchasableProduct product)
        {
            var itemPrefab = GetPrefab(product);
            var itemGameObject = Instantiate(itemPrefab, _container);
            var component = itemGameObject.GetComponent<IProductItemView>();
            component.Selected += OnItemClicked;

            if (component is ICurrencyProductItemView currencyComponent)
            {
                currencyComponent.NotEnoughCurrencyForPurchase += OnNotEnoughCurrencyForPurchase;
            }

            _productItems.Add(component);
            _gridBottomSpacer.SetAsLastSibling();

            return component;
        }

        private void OnNotEnoughCurrencyForPurchase(PurchasableProduct product)
        {
            NotEnoughFundsForPurchase?.Invoke(product);
        }

        private void OnItemClicked(PurchasableProduct product)
        {
            ProductSelected?.Invoke(product);
        }

        private void HideAllProducts()
        {
            _premiumPassPurchasedInstance?.SetActive(false);
            
            foreach (var productItem in _productItems)
            {
                productItem.Hide();
            }
        }

        private GameObject GetPrefab(PurchasableProduct model)
        {
            switch (model.Type)
            {
                case ProductType.HardCurrency:
                    return _gemCurrencyItemViews.First(x => string.Equals(x.ItemIdentifier, model.ReferenceId, StringComparison.OrdinalIgnoreCase)).gameObject;
                case ProductType.SoftCurrency:
                    return _coinCurrencyItemViews.First(x => string.Equals(x.ItemIdentifier, model.ReferenceId, StringComparison.OrdinalIgnoreCase)).gameObject;
                case ProductType.SeasonPass:
                    return _premiumPass.gameObject;
                default:
                    throw new ArgumentOutOfRangeException(nameof(model.Type), model.Type, null);
            }
        }
    }
}