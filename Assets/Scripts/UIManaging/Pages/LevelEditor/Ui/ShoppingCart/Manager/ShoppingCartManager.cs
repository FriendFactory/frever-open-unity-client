using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.Common;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.ShoppingCart
{
    public abstract class ShoppingCartManager : MonoBehaviour
    {
        [SerializeField] private ShoppingCartPanel _shoppingCartPrefab;

        private ShoppingCartPanel _shoppingCartPanel;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action AssetsUpdated;
        public event Action BuyButtonClicked;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public List<IPurchasable> ConfirmedAssets { get; private set; } = new List<IPurchasable>();
        
        public int TotalSoftPrice { get; private set; }
        public int TotalHardPrice { get; private set; }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnDestroy()
        {
            if (_shoppingCartPanel == null) return;
            
            _shoppingCartPanel.BuyButtonClicked -= OnBuyButtonClicked;
            _shoppingCartPanel.PanelClosed -= OnPanelClosed;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public virtual void ShowCart()
        {
            if (_shoppingCartPanel == null)
            {
                _shoppingCartPanel = Instantiate(_shoppingCartPrefab, transform);
                _shoppingCartPanel.transform.SetAsFirstSibling();
                _shoppingCartPanel.Init(this);
                _shoppingCartPanel.BuyButtonClicked += OnBuyButtonClicked;
                _shoppingCartPanel.PanelClosed += OnPanelClosed;
            }

            _shoppingCartPanel.Show();
        }

        public int GetSoftPriceForSelectedAssets()
        {
            return GetSelectedAssets()
                ?.Sum(asset => asset.AssetOffer.AssetOfferSoftCurrencyPrice ?? 0)
                ?? 0;
        }
        
        public int GetHardPriceForSelectedAssets()
        {
            return GetSelectedAssets()
              ?.Sum(asset => asset.AssetOffer.AssetOfferHardCurrencyPrice ?? 0)
              ?? 0;
        }
        
        public abstract List<IPurchasable> GetSelectedAssets();
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected void UpdateConfirmedAssets()
        {
            ConfirmedAssets = GetStoreAssets();
            TotalSoftPrice = ConfirmedAssets.Sum(asset => asset.AssetOffer.AssetOfferSoftCurrencyPrice ?? 0);
            TotalHardPrice = ConfirmedAssets.Sum(asset => asset.AssetOffer.AssetOfferHardCurrencyPrice ?? 0);

            AssetsUpdated?.Invoke();
        }
        
        protected virtual void OnPanelClosed()
        {
        }

        protected abstract List<IPurchasable> GetStoreAssets();

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnBuyButtonClicked()
        {
            BuyButtonClicked?.Invoke();
        }
    }
}