using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bridge;
using Bridge.Models.Common;
using Modules.Amplitude;
using I2.Loc;
using TMPro;
using UIManaging.Localization;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UIManaging.PopupSystem;

namespace Common.ShoppingCart
{
    public sealed class ShoppingCart : MonoBehaviour
    {
        private Action _itemsPurchasingRequested;

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Color _notEnoughCurrencyColor;
        [SerializeField] private ShoppingCartItem _itemPrefab;
        [SerializeField] private Transform _container;
        [SerializeField] private TextMeshProUGUI _itemsTotalText;
        [SerializeField] private TextMeshProUGUI _softPriceText;
        [SerializeField] private TextMeshProUGUI _hardPriceText;
        [SerializeField] private Button _buyButton;
        [SerializeField] private TextMeshProUGUI _buyButtonText;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _outsideClickButton;
        [SerializeField] private bool _closeOnBuy = true;
        [SerializeField] private bool _restoreItemsOnClose = true;
        
        [SerializeField] private LocalizedString _itemsTitleFormat;

        [Inject] private IBridge _bridge;
        [Inject] private AmplitudeManager _amplitudeManager;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private LocalUserDataHolder _userData;
        [Inject] private ShoppingCartLocalization _localization;
        
        private readonly List<ShoppingCartItem> _cartItems = new();

        private int _softTotal;
        private int _hardTotal;
        private Action _onShown;
        private Color? _defaultCurrencyTextColor;
        private bool _isOnboarding;

        // Workaround to prevent simultaneous interactions with shopping cart items and the buy button
        private bool _isButtonClickedThisFrame;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<IEntity, bool> ItemSelectionChanged;
        public event Action ShoppingCartClosed;
        public event Action PurchasingItemsRequested;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private Color DefaultCurrencyTextColor
        {
            get
            {
                _defaultCurrencyTextColor ??= _softPriceText.faceColor;
                return _defaultCurrencyTextColor.Value;;
            }
        }

        private Bridge.Services.UserProfile.UserBalance UserBalance => _userData.UserBalance;
        private bool HasEnoughSoftCurrency => _softTotal <= 0 || UserBalance.SoftCurrencyAmount >= _softTotal;
        private bool HasEnoughHardCurrency => _hardTotal <= 0 || UserBalance.HardCurrencyAmount >= _hardTotal;
        private bool HasEnoughCurrency => HasEnoughSoftCurrency && HasEnoughHardCurrency;
        public bool IsShown => gameObject.activeInHierarchy;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _buyButton.onClick.AddListener(OnBuyClicked);
            _closeButton.onClick.AddListener(Close);
            _outsideClickButton.onClick.AddListener(CloseAndRestoreItems);
            _buyButtonText = _buyButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Show(bool isOnboarding, Action onShown = null)
        {
            _isOnboarding = isOnboarding;
            _userData.UserBalanceUpdated -= OnUserBalanceUpdated;
            _userData.UserBalanceUpdated += OnUserBalanceUpdated;
            
            gameObject.SetActive(true);
            _onShown = onShown;
            _closeButton.enabled = true;
            _outsideClickButton.enabled = true;
            StartCoroutine(ShownDelay());
        }

        public void Close()
        {
            gameObject.SetActive(false);

            // TODO: pooling instead of destroying
            foreach (var item in _cartItems)
            {
                Destroy(item.gameObject);
            }
            _cartItems.Clear();
            _softTotal = 0;
            _hardTotal = 0;
            _userData.UserBalanceUpdated -= OnUserBalanceUpdated;
            ShoppingCartClosed?.Invoke();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void CloseAndRestoreItems()
        {
            if (_restoreItemsOnClose)
            {
                RestoreItems();
            }
            
            Close();
            
            void RestoreItems()
            {
                foreach (var item in _cartItems)
                {
                    if (item.IsSelected) continue;
                    item.OnToggleChanged(true);
                }
            }
        }

        private void OnBuyClicked()
        {
            if (_isButtonClickedThisFrame) return;
            _isButtonClickedThisFrame = true;

            if (HasEnoughCurrency)
            {
                OnPurchasingItemsRequested();
            }
            else
            {
                OnNotEnoughCurrencyToBuyAssets();
            }

            ResetButtonStateNextFrame();
        }

        private void OnNotEnoughCurrencyToBuyAssets()
        {
            if (!_isOnboarding)
            {
                _popupManagerHelper.ShowNotEnoughFundsPopupWithBuyOption(() =>
                {
                    ShowShop();
                    _popupManagerHelper.HideShowNotEnoughFundsPopupWithBuyOption();
                });
            }
            else
            {
                _popupManagerHelper.ShowNotEnoughFundsPopup(_isOnboarding);
            }
        }

        public void Setup<T>(ICollection<T> entities, Action onComplete) where T : IThumbnailOwner, IMinLevelRequirable, IPurchasable, INamed
        {
            _itemsPurchasingRequested = onComplete;
            foreach (var item in entities)
            {
                var cartItem = Instantiate(_itemPrefab, _container);
                cartItem.Init(_bridge);
                cartItem.Setup(item);
                cartItem.SelectionChanged += OnItemSelectionChanged;
                _cartItems.Add(cartItem);
            }
            UpdateTotal();
            Unlock();
            SendOpenShoppingCartEvent();
        }

        public void Lock()
        {
            _buyButton.interactable = false;
            foreach (var item in _cartItems)
            {
                item.SetInteractable(false);
            }
            
            _canvasGroup.interactable = false;
        }

        public void Unlock()
        {
            if (_userData == null) return;
            foreach (var item in _cartItems)
            {
                item.SetInteractable(true);
            }
            UpdateBuyButtonState();
            
            _canvasGroup.interactable = true;
        }

        private void OnItemSelectionChanged(IEntity item, bool selected)
        {
            if (_isButtonClickedThisFrame) return;
            _isButtonClickedThisFrame = true;

            ItemSelectionChanged?.Invoke(item, selected);
            UpdateTotal();
            Lock();

            ResetButtonStateNextFrame();
        }

        private void UpdateTotal()
        {
            _softTotal = 0;
            _hardTotal = 0;
            var selectedCount = 0;
            foreach (var item in _cartItems)
            {
                if (!item.IsSelected) continue;

                selectedCount++;

                if (item.AssetOffer == null) continue;            

                if (item.AssetOffer.AssetOfferSoftCurrencyPrice.HasValue)
                {
                    _softTotal += item.AssetOffer.AssetOfferSoftCurrencyPrice.Value;
                }

                if (item.AssetOffer.AssetOfferHardCurrencyPrice.HasValue)
                {
                    _hardTotal += item.AssetOffer.AssetOfferHardCurrencyPrice.Value;
                }
            }

            _itemsTotalText.text = string.Format(_itemsTitleFormat, selectedCount);
            _softPriceText.text = $"<sprite index=0> {_softTotal}";
            _hardPriceText.text = $"<sprite index=1> {_hardTotal}";
            LayoutRebuilder.ForceRebuildLayoutImmediate(_softPriceText.transform.parent as RectTransform);
            UpdateBuyButtonState();
        }

        private void UpdateBuyButtonState()
        {
            _softPriceText.color = HasEnoughSoftCurrency ? DefaultCurrencyTextColor : _notEnoughCurrencyColor;
            _hardPriceText.color = HasEnoughHardCurrency ? DefaultCurrencyTextColor : _notEnoughCurrencyColor;

            var hasLockedItems = IsLockedItemSelected();
            if (hasLockedItems)
            {
                _buyButtonText.text = _localization.BuyButtonLocked;
            }
            else if (_softTotal > 0 || _hardTotal > 0)
            {
                _buyButtonText.text = _localization.BuyButtonBuy;
            }
            else
            {
                _buyButtonText.text = _localization.BuyButtonDone;
            }

            _buyButton.interactable = !hasLockedItems;
        }

        private bool IsLockedItemSelected()
        {
            var hasLocked = false;
            var currentLevel = 0;
            if (_userData.LevelingProgress?.Xp.CurrentLevel != null)
            {
                currentLevel = _userData.LevelingProgress.Xp.CurrentLevel.Level;
            }
        
            foreach (var item in _cartItems)
            {
                if (item.IsSelected && item.MinRequiredLevel > currentLevel)
                {
                    hasLocked = true;
                    break;
                }
            }
            return hasLocked;
        }

        private IEnumerator ShownDelay()
        {
            yield return null;
            _onShown?.Invoke();
        }

        private void SendOpenShoppingCartEvent()
        {
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.OPEN_CART, GetCartAssetsMetaData());
        }
    
        private void SendShoppingCartPurchaseEvent()
        {
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.PURCHASE_ITEMS, GetCartAssetsMetaData());
        }

        private Dictionary<string, object> GetCartAssetsMetaData()
        {
            var cartAssetIds = _cartItems.Where(x=>x.IsSelected).Select(x => x.AssetOffer.AssetId).ToArray();
            var cartAssetHardCurrencyPrices = _cartItems.Select(x => x.AssetOffer.AssetOfferHardCurrencyPrice).ToArray();
            var cartAssetSoftCurrencyPrices = _cartItems.Select(x => x.AssetOffer.AssetOfferSoftCurrencyPrice).ToArray();
            var metaData = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.ASSET_IDS] = string.Join(",", cartAssetIds),
                [AmplitudeEventConstants.EventProperties.HARD_CURRENCY_PRICE] = string.Join(",", cartAssetHardCurrencyPrices),
                [AmplitudeEventConstants.EventProperties.SOFT_CURRENCY_PRICE] = string.Join(",", cartAssetSoftCurrencyPrices),
            };

            return metaData;
        }

        private void OnPurchasingItemsRequested()
        {
            SendShoppingCartPurchaseEvent();
            _itemsPurchasingRequested?.Invoke();
            
            PurchasingItemsRequested?.Invoke();

            _closeButton.enabled = false;
            _outsideClickButton.enabled = false;
            if (_closeOnBuy)
            {
                Close();
            }
        }

        private void ShowShop()
        {
            _popupManagerHelper.ShowStorePopup(false);
        }
        
        private void OnUserBalanceUpdated()
        {
            UpdateBuyButtonState();
        }

        private void ResetButtonStateNextFrame()
        {
            CoroutineSource.Instance.ExecuteWithFrameDelay(() =>
            {
                _isButtonClickedThisFrame = false;
            });
        }
    }
}