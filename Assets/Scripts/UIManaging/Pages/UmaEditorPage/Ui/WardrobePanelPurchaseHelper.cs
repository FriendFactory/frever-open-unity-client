using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Extensions;
using Modules.Amplitude.Signals;
using Modules.FreverUMA;
using Modules.WardrobeManaging;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.UmaEditorPage.Ui.Amplitude;
using UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel;
using UIManaging.PopupSystem;
using UIManaging.SnackBarSystem;
using UIManaging.SnackBarSystem.Configurations;
using UnityEngine;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    public sealed class WardrobePanelPurchaseHelper: MonoBehaviour
    {
        [SerializeField] private WardrobePanelUIBase _wardrobePanel;
        [SerializeField] private UmaEditorPageLocalization _localization;
        [SerializeField] private GameObject _notEnoughMoneyIcon;
        [SerializeField] private ShoppingBagCounterUI _bagCounterUI;
        
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private LocalUserDataHolder _userDataHolder;
        [Inject] private IBridge _bridge;
        [Inject] private WardrobesResponsesCache _wardrobesResponsesCache;
        [Inject] private SnackBarManager _snackBarManager;
        [Inject] private ClothesCabinet _clothesCabinet;
        [Inject] private ICharacterEditor _characterEditor;
        [Inject] private IWardrobeStore _wardrobeStore;
        [Inject] private INotOwnedWardrobesProvider _notOwnedWardrobesProvider;
        [Inject] private SignalBus _signalBus;

        private CancellationTokenSource _cancellationTokenSource;
        
        public async void Purchase(IEntity purchasedAsset)
        {
            if (purchasedAsset is WardrobeShortInfo wardrobeItem)
            {
                if (_userDataHolder.UserBalance.SoftCurrencyAmount <
                    wardrobeItem.AssetOffer.AssetOfferSoftCurrencyPrice
                 || _userDataHolder.UserBalance.HardCurrencyAmount <
                    wardrobeItem.AssetOffer.AssetOfferHardCurrencyPrice)
                {
                    _popupManagerHelper.ShowNotEnoughFundsPopupWithBuyOption(() =>
                    {
                        _popupManagerHelper.ShowStorePopup(false);
                        _popupManagerHelper.HideShowNotEnoughFundsPopupWithBuyOption();
                    });
                    _wardrobePanel.UpdateLoadingState(null);

                    return;
                }

                _wardrobePanel.UpdateLoadingState(purchasedAsset);

                var result = await _wardrobeStore.Purchase(wardrobeItem.AssetOffer.Id);

                if (result.IsError)
                {
                    Debug.LogError($"Failed to purchase asset {wardrobeItem.Id}, reason: {result.ErrorMessage}");
                    _wardrobePanel.UpdateLoadingState(null);
                    return;
                }

                if (result.IsSuccess)
                {
                    if (result.Model.Ok)
                    {
                        _wardrobesResponsesCache.Invalidate();
                        _wardrobePanel.UpdateAfterPurchase(purchasedAsset);
                        UpdateBalance();
                        UpdateShoppingCartIconsInternal();
                        LoadThumbnail(wardrobeItem);
                        
                        _signalBus.Fire(new AmplitudeEventSignal(new WardrobeItemPurchasedAmplitudeEvent(wardrobeItem)));
                    }
                    else
                    {
                        Debug.LogError($"Failed to purchase asset {wardrobeItem.Id}, reason: {result.Model.ErrorMessage}");
                        _wardrobePanel.UpdateLoadingState(null);
                    }
                }
            }
        }

        private async void UpdateBalance()
        {
            await _userDataHolder.UpdateBalance();
        }

        async void LoadThumbnail(IFilesAttachedEntity entity)
        {
            _cancellationTokenSource?.CancelAndDispose();
            _cancellationTokenSource = new CancellationTokenSource();
            
            var result = await _bridge.GetThumbnailAsync(entity, Resolution._256x256, cancellationToken: _cancellationTokenSource.Token);

            if (result.IsRequestCanceled) return;

            if (result.IsSuccess)
            {
                OnThumbnailLoaded(result.Object);
            }
            else
            {
                Debug.LogWarning(result.ErrorMessage);
            }

            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        private void OnThumbnailLoaded(object obj)
        {
            if (obj is Texture2D thumbnailTexture)
            {
                var rect = new Rect(0.0f, 0.0f, thumbnailTexture.width, thumbnailTexture.height);
                var pivot = new Vector2(0.5f, 0.5f);
                var sprite = Sprite.Create(thumbnailTexture, rect, pivot);
                var assetClaimedConfiguration = new AssetSnackBarConfiguration(_localization.AssetPurchasedSnackbarTitle, _localization.AssetPurchasedSnackbarDesc, sprite);

                _snackBarManager.Show(assetClaimedConfiguration);
            }
            else
            {
                Debug.LogWarning("Wrong thumbnail format");
            }
        }
        
        private void UpdateShoppingCartIconsInternal()
        {
            if (_notEnoughMoneyIcon)
            {
                _notEnoughMoneyIcon.SetActive(!CheckSufficientFunds());
            }
            
            if (_bagCounterUI)
            {
                var notOwnedList = GetNotOwnedWardrobes();
                _bagCounterUI.SetBagNumber(notOwnedList.Count);
            }
        }
        
        public List<WardrobeShortInfo> GetNotOwnedWardrobes()
        {
            return _notOwnedWardrobesProvider.GetNotOwnedWardrobes().ToList();
        }
        
        public bool CheckSufficientFunds()
        {
            var notOwnedList = GetNotOwnedWardrobes();
            var neededSoft = 0;
            var neededHard = 0;
            var hasLockedItem = false;

            var currentLevel = 0;
            if (_userDataHolder.LevelingProgress != null)
            {
                var levelObj = _userDataHolder?.LevelingProgress.Xp.CurrentLevel;
                if (levelObj != null) currentLevel = levelObj.Level;
            }

            foreach (var wardrobe in notOwnedList)
            {
                var assetOffer = wardrobe.AssetOffer;
                if (assetOffer == null) continue;

                if (assetOffer.AssetOfferSoftCurrencyPrice.HasValue)
                    neededSoft += assetOffer.AssetOfferSoftCurrencyPrice.Value;

                if (assetOffer.AssetOfferHardCurrencyPrice.HasValue)
                    neededHard += assetOffer.AssetOfferHardCurrencyPrice.Value;

                var requiredLevel = wardrobe.SeasonLevel;
                if (requiredLevel.HasValue && requiredLevel.Value > currentLevel)
                    hasLockedItem = true;
            }

            return !(neededSoft > 0 && neededSoft > _userDataHolder.UserBalance.SoftCurrencyAmount
                  || neededHard > 0 && neededHard > _userDataHolder.UserBalance.HardCurrencyAmount ||
                     hasLockedItem);
        }
    }
}