using System;
using System.Threading;
using Abstract;
using Bridge;
using Bridge.Models.ClientServer.Gamification;
using Extensions;
using Extensions.DateTime;
using I2.Loc;
using Modules.AssetsStoraging.Core;
using Modules.InAppPurchasing;
using TMPro;
using UIManaging.PopupSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Popups.Store.SeasonPassProposal
{
    public sealed class SeasonPassItemView : BaseContextDataView<SeasonPassProduct>, IProductItemView
    {
        [SerializeField] private Button _exploreButton;
        [SerializeField] private Image _image;
        [SerializeField] private RewardsList _rewardsList;
        [SerializeField] private TextMeshProUGUI _timeText;
        [Header("Localization")]
        [SerializeField] private LocalizedString _timeLeftFormat;
        
        [Inject] private ISeasonProvider _seasonProvider;
        [Inject] private IStorageBridge _bridge;
        [Inject] private PopupManagerHelper _popupManagerHelper;

        private CancellationTokenSource _cancellationTokenSource;
        private bool _hasThumbnail;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<PurchasableProduct> Selected;
        public event Action OnSuccessfulPassPurchase;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
       
        private SeasonReward[] PremiumRewards => _seasonProvider.CurrentSeason.GetPremiumLevelRewards();
        public PurchasableProduct Product => ContextData;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _exploreButton.onClick.AddListener(OnExploreClicked);
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Initialize(PurchasableProduct product)
        {
            if (product is SeasonPassProduct seasonPassProduct)
            {
                base.Initialize(seasonPassProduct);
            }
            else
            {
                throw new InvalidOperationException($"SeasonPassItemView requires context data type: {nameof(SeasonPassProduct)}");
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            var alreadyInitializedRewards = !_rewardsList.Empty;
            if (!alreadyInitializedRewards)
            {
                _rewardsList.Setup(PremiumRewards);
            }
            LoadPremiumPassThumbnail();
            _timeText.text = string.Format(_timeLeftFormat, _seasonProvider.CurrentSeason.EndDate.GetFormattedTimeLeft());
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private async void LoadPremiumPassThumbnail()
        {
            if (_hasThumbnail) return;
            
            if (ContextData.ProductOffer.Files == null) return;

            _cancellationTokenSource = new CancellationTokenSource();
            
            var thumbnailResp = await _bridge.GetThumbnailAsync(ContextData.ProductOffer, Resolution._256x256, true, _cancellationTokenSource.Token);
            if (thumbnailResp.IsError)
            {
                Debug.LogWarning($"Couldn't load the season thumbnail. Reason: {thumbnailResp.ErrorMessage}");
                return;
            }

            if (thumbnailResp.IsRequestCanceled)
            {
                return;
            }

            var texture = thumbnailResp.Object as Texture2D;
            _image.sprite = texture.ToSprite();
            _image.SetAlpha(1f);
            _image.preserveAspect = true;
            _hasThumbnail = true;
        }

        private void OnExploreClicked()
        {
            Selected?.Invoke(ContextData);
            _popupManagerHelper.ShowPremiumPassPopup(OnSuccessfulPassPurchase);
        }
    }
}