using System;
using System.Collections.Generic;
using Bridge.Models.Common;
using JetBrains.Annotations;
using TMPro;
using UIManaging.Localization;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.ShoppingCart
{
    public class ShoppingCartPanel : MonoBehaviour
    {
        [SerializeField] private Button _dimmerButton;
        [Space]
        [SerializeField] private ShoppingCartListView _listView;
        [SerializeField] private TextMeshProUGUI _totalItemsText;
        [Space]
        [SerializeField] private TextMeshProUGUI _softPriceText;
        [SerializeField] private GameObject _softPriceRedFlag;
        [Space]
        [SerializeField] private TextMeshProUGUI _hardPriceText;
        [SerializeField] private GameObject _hardPriceRedFlag;
        [Space]
        [SerializeField] private Button _buyButton;

        [Inject] private ShoppingCartLocalization _localization;
        
        private ShoppingCartManager _cartManager;
        private LocalUserDataHolder _userData;
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action BuyButtonClicked;
        public event Action PanelClosed;
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        [Inject, UsedImplicitly]
        private void Construct(LocalUserDataHolder userDataHolder)
        {
            _userData = userDataHolder;
        }

        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _dimmerButton.onClick.AddListener(Hide);
            _buyButton.onClick.AddListener(OnBuyButtonClicked);
        }

        private void OnDestroy()
        {
            _dimmerButton.onClick.RemoveListener(Hide);
            _buyButton.onClick.RemoveListener(OnBuyButtonClicked);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(ShoppingCartManager cartManager)
        {
            _cartManager = cartManager;
        }

        public virtual void Show()
        {
            InitializeListView();
            UpdateItemsCount();
            UpdateTotalPrice();

            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
            PanelClosed?.Invoke();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void InitializeListView()
        {
            var confirmedAssets = _cartManager.ConfirmedAssets;
            var selectedAssets = _cartManager.GetSelectedAssets();
            var itemModels = GetItemModels(confirmedAssets, selectedAssets);
            var listModel = new ShoppingCartListModel(itemModels);
            _listView.Initialize(listModel);
        }

        private static ShoppingCartItemModel[] GetItemModels(IReadOnlyList<IPurchasable> confirmedAssets, IReadOnlyList<IPurchasable> selectedAssets)
        {
            var hasSelectedAssets = selectedAssets?.Count > 0;
            var modelsCount = (hasSelectedAssets)
                ? confirmedAssets.Count + selectedAssets.Count + 1
                : confirmedAssets.Count;

            var models = new ShoppingCartItemModel[modelsCount];

            for (var i = 0; i < confirmedAssets.Count; i++)
            {
                models[i] = new ShoppingCartAssetModel(confirmedAssets[i]);
            }

            if (!hasSelectedAssets) return models;

            models[confirmedAssets.Count] = new ShoppingCartHeaderModel("↓ Current selected assets ↓");

            var indexOffset = confirmedAssets.Count + 1;
            for (var i = 0; i < selectedAssets.Count; i++)
            {
                models[indexOffset + i] = new ShoppingCartAssetModel(selectedAssets[i], false);
            }

            return models;
        }

        private void UpdateItemsCount()
        {
            var items = _listView.ContextData.AssetItems;
            var itemsCount = items.Length.ToString();
            _totalItemsText.text = string.Format(_localization.ItemsCounterTextFormat, itemsCount);
        }

        private void UpdateTotalPrice()
        {
            var softCurrencyAmount = _userData.UserBalance.SoftCurrencyAmount;
            var softPrice = _cartManager.TotalSoftPrice + _cartManager.GetSoftPriceForSelectedAssets();
            _softPriceText.text = softPrice.ToString();
            _softPriceRedFlag.SetActive(softPrice > 0 && softCurrencyAmount < softPrice);
            
            var hardCurrencyAmount = _userData.UserBalance.HardCurrencyAmount;
            var hardPrice = _cartManager.TotalHardPrice + _cartManager.GetHardPriceForSelectedAssets();
            _hardPriceText.text = hardPrice.ToString();
            _hardPriceRedFlag.SetActive(hardPrice > 0 && hardCurrencyAmount < hardPrice);
        }
        
        private void OnBuyButtonClicked()
        {
            BuyButtonClicked?.Invoke();
            Hide();
        }
    }
}