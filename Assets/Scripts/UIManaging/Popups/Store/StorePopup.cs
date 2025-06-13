using System;
using Common;
using Common.UserBalance;
using Modules.InAppPurchasing;
using Navigation.Args;
using Navigation.Core;
using Sirenix.OdinInspector;
using UIManaging.Localization;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Tasks.RewardPopUp;
using UIManaging.Popups.Store.SuccessWindow;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Popups.Store
{
    internal sealed class StorePopup : BasePopup<StorePopupConfiguration>
    {
        [SerializeField] private CurrencyPurchaseSuccessWindow _currencyPurchaseSuccessWindow;
        [SerializeField] private FlyingRewardsAnimationController _flyingRewardsAnimationController;
        [SerializeField] private UserBalanceView _animatedUserBalance;
        [SerializeField] private StoreProductsListView _storeProductsListView;
        [SerializeField] private Button _closeButton;
        [SerializeField] private GameObject _loadingOverlay;
        [SerializeField] private Canvas _wallerSortOrderControl;
        [SerializeField] private SupportCreatorView _supportCreatorView;

        [Inject] private IIAPManager _iapManager;
        [Inject] private LocalUserDataHolder _localUserDataHolder;
        [Inject] private SnackBarHelper _snackbarHelper;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private PageManager _pageManager;
        [Inject] private PurchasesLocalization _purchasesLocalization;

        private Action _onClosed;
        private bool _multiTouchBeforeOpening;
        private bool _showSeasonRewardsButton;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _storeProductsListView.ProductSelected += OnProductSelected;
            _storeProductsListView.NotEnoughFundsForPurchase += OnNotEnoughCurrencyForPurchase;
        }

        private void OnDestroy()
        {
            _storeProductsListView.ProductSelected -= OnProductSelected;
            _storeProductsListView.NotEnoughFundsForPurchase -= OnNotEnoughCurrencyForPurchase;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public override void Show()
        {
            _multiTouchBeforeOpening = Input.multiTouchEnabled;
            Input.multiTouchEnabled = false;
            base.Show();
            ShowProducts();

            _animatedUserBalance.Initialize(new StaticUserBalanceModel(_localUserDataHolder));

            _supportCreatorView.Initialize();
            
            _iapManager.PurchaseStarted += OnPurchaseStarted;
            _iapManager.PurchaseConfirmed += OnPurchaseConfirmed;
            _iapManager.PurchaseSuccess += OnPurchaseSuccess;
            _iapManager.PurchaseFailed += OnPurchaseFailed;
            _closeButton.onClick.AddListener(Hide);
            SetWalletToDefaultSortingLayer();
        }

        public override void Hide()
        {
            Input.multiTouchEnabled = _multiTouchBeforeOpening;
            _iapManager.PurchaseStarted -= OnPurchaseStarted;
            _iapManager.PurchaseConfirmed -= OnPurchaseConfirmed;
            _iapManager.PurchaseFailed -= OnPurchaseFailed;
            _iapManager.PurchaseSuccess -= OnPurchaseSuccess;
            _closeButton.onClick.RemoveListener(Hide);
            _onClosed?.Invoke();
            base.Hide();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnConfigure(StorePopupConfiguration configuration)
        {
            _onClosed = configuration.OnClosed;
            _showSeasonRewardsButton = configuration.ShowSeasonRewardsButtonOnPremiumPassPurchase;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnPurchaseStarted()
        {
            _loadingOverlay.SetActive(true);
        }

        private void OnPurchaseConfirmed(PurchasableProduct product)
        {
            _loadingOverlay.SetActive(false);
            
            switch (product.Type)
            {
                case ProductType.SoftCurrency:
                    SetWalletToDefaultSortingLayer();
                    _currencyPurchaseSuccessWindow.Show(product.Amount.ToString(), product.Type, OnSoftCurrencyConfirm);
                    break;
                case ProductType.HardCurrency:
                    SetWalletToDefaultSortingLayer();
                    _currencyPurchaseSuccessWindow.Show(product.Amount.ToString(), product.Type, OnHardCurrencyConfirm);
                    break;
                case ProductType.SeasonPass:
                    _popupManagerHelper.HidePremiumPassPopup();
                    ShowPurchasedPremiumPassPopup();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            void OnHardCurrencyConfirm()
            {
                PlayHardCurrencyAnimation(product);
                _localUserDataHolder.AddHardCurrency(product.Amount.Value);
            }
            void OnSoftCurrencyConfirm()
            {
                _localUserDataHolder.AddHardCurrency(-int.Parse(product.PriceText));
                PlaySoftCurrencyAnimation(product);
                _localUserDataHolder.AddSoftCurrency(product.Amount.Value);
            }
        }

        private void ShowPurchasedPremiumPassPopup()
        {
            void OnSeasonRewardsButtonClicked()
            {
                _popupManagerHelper.HidePremiumPassPurchaseSucceedPopup();
                _popupManagerHelper.HideStorePopup();
                var args = new SeasonPageArgs(SeasonPageArgs.Tab.Rewards);
                _pageManager.MoveNext(args);
            }

            _popupManagerHelper.ShowPremiumPassPurchaseSucceedPopup(_showSeasonRewardsButton, OnSeasonRewardsButtonClicked, ShowProducts);
        }

        private void OnPurchaseSuccess()
        {
            _snackbarHelper.ShowPurchaseSuccessSnackBar(_purchasesLocalization.PurchaseCompletedMessage);
        }
        
        private void OnPurchaseFailed()
        {
            _loadingOverlay.SetActive(false);
            _snackbarHelper.ShowPurchaseFailedSnackBar(_purchasesLocalization.PurchaseFailedMessage);
        }

        private void PlaySoftCurrencyAnimation(PurchasableProduct product)
        {
            var userBalance = _localUserDataHolder.UserBalance;
            var fromSoft = userBalance.SoftCurrencyAmount;
            var toSoft = userBalance.SoftCurrencyAmount + product.Amount.Value;
            var hardBalance = _localUserDataHolder.UserBalance.HardCurrencyAmount;
            
            var args = GetBalanceArgs(fromSoft, toSoft, hardBalance, hardBalance);
            
            _animatedUserBalance.Initialize(new AnimatedUserBalanceModel(args));
            _flyingRewardsAnimationController.Play(true, false, false);
            
        }

        private void PlayHardCurrencyAnimation(PurchasableProduct product)
        {
            var userBalance = _localUserDataHolder.UserBalance;
            var fromHard = userBalance.HardCurrencyAmount;
            var toHard = userBalance.HardCurrencyAmount + product.Amount.Value;
            var softBalance = _localUserDataHolder.UserBalance.SoftCurrencyAmount;

            var args = GetBalanceArgs(softBalance, softBalance, fromHard, toHard);
            
            _animatedUserBalance.Initialize(new AnimatedUserBalanceModel(args));
            _flyingRewardsAnimationController.Play(false, true, false);
            
        }


        private UserBalanceArgs GetBalanceArgs(int fromSoft, int toSoft, int fromHard, int toHard)
        {
            return new UserBalanceArgs(1.5f, 0.5f, fromSoft, toSoft, fromHard, toHard);
        }
        
        private void OnProductSelected(PurchasableProduct product)
        {
            if (product.Type != ProductType.SoftCurrency)
            {
                SetWalletToDefaultSortingLayer();
            }
            else
            {
                _wallerSortOrderControl.overrideSorting = true;
                _wallerSortOrderControl.sortingOrder = Constants.POPUPS_SORTING_LAYER + 1;
            }
        }
        
        private void OnNotEnoughCurrencyForPurchase(PurchasableProduct obj)
        {
            SetWalletToDefaultSortingLayer();
        }

        private void SetWalletToDefaultSortingLayer()
        {
            _wallerSortOrderControl.overrideSorting = false;
        }
        
        private void ShowProducts()
        {
            var products = _iapManager.GetAvailableProducts();
            var listModel = new StoreProductsListModel
            {
                Products = products
            };
            _storeProductsListView.Initialize(listModel);
        }
        
        [Button]
        private void TestSoftCurrencyFlow()
        {
            var product = new SoftCurrencyProduct()
            {
                PriceText = "10",
                Amount = 200
            };
            OnPurchaseConfirmed(product);
        }

        [Button]
        private void TestHardCurrencyFlow()
        {
            var product = new HardCurrencyProduct()
            {
                PriceText = "10",
                Amount = 200
            };
            OnPurchaseConfirmed(product);
        }
    }
}