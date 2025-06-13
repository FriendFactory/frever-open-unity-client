using System;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using UnityEngine;
using Common.UI;
using Extensions;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    public class WardrobeUIItem : ImagedWardrobeUIItem<IThumbnailOwner>
    {
        public event Action<IEntity> PurchaseRequested;
        
        [SerializeField] private GameObject _selectionGameObject;
        [SerializeField] private PriceHolder _priceHolder;
        [SerializeField] private BadgeRequirementUI _badgeRequirementUI;
        [SerializeField] protected TierBackgroundUI _tierBackgroundUI;
        [SerializeField] private Button _selectBtn;
        [SerializeField] private Button _buyBtn;
        [SerializeField] private Button _confirmBtn;
        [SerializeField] private GameObject _buyBtnObj;
        [SerializeField] private GameObject _confirmBtnObj;

        [Inject] private LocalUserDataHolder _dataHolder;
        
        private bool _isOwned;
        protected bool _hasTier;

        public override bool IsLoading
        {
            get => base.IsLoading;
            set
            {
                base.IsLoading = value;
                SetActiveLoading(value);
            }
        }
        public bool IsOwned
        {
            get => _isOwned;
            set
            {
                _isOwned = value;
                SetupPrice();
            }
        }

        private bool _initialized;
        private WardrobesInputHandler _wardrobesInputHandler;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_selectBtn != null)
            {
                _selectBtn.onClick.RemoveListener(OnSelectButtonClicked);
            }
            
            _buyBtn.onClick.RemoveListener(OnBuyButtonClicked);
            _confirmBtn.onClick.RemoveListener(OnConfirmButtonClicked);
        }
        
        public override void Init(IBridge bridge)
        {
            if (_initialized)
            {
                return;
            }
            
            base.Init(bridge);

            if (_selectBtn != null)
            {
                _selectBtn.onClick.AddListener(OnSelectButtonClicked);
            }

            _buyBtn.onClick.AddListener(OnBuyButtonClicked);
            _confirmBtn.onClick.AddListener(OnConfirmButtonClicked);
            
            _initialized = true;
        }

        public override void Setup(IThumbnailOwner entity)
        {
            base.Setup(entity);
            
            _buyBtnObj.SetActive(false);
            _confirmBtnObj.SetActive(false);
            
            SetupBadge();
            SetupTier();
            SetupPrice();
        }

        public void ClearEvents()
        {
            ClearSelectedEvent();

            if (PurchaseRequested == null)
            {
                return;
            }

            foreach (var handler in PurchaseRequested.GetInvocationList())
            {
                PurchaseRequested -= handler as Action<IEntity>;
            }
        }

        public void SetupInputHandler(WardrobesInputHandler wardrobesInputHandler)
        {
            _wardrobesInputHandler = wardrobesInputHandler;
        }

        protected override void OnItemSelected()
        {
            ChangeSelectionVisual();
            
            base.OnItemSelected();
        }

        protected override void ChangeSelectionVisual()
        {
            base.ChangeSelectionVisual();

            var hasPrice = Entity is WardrobeShortInfo wardrobe 
                        && wardrobe.AssetOffer != null 
                        && (wardrobe.AssetOffer.AssetOfferSoftCurrencyPrice != null ||
                            wardrobe.AssetOffer.AssetOfferHardCurrencyPrice != null);
            
            _selectionGameObject.SetActive(Selected);
            _buyBtnObj.SetActive((_dataHolder?.IsOnboardingCompleted ?? false) && !(_dataHolder?.IsStarCreator ?? true) && hasPrice && Selected && !IsOwned && !IsLoading);
            _confirmBtnObj.SetActive(false);
        }

        protected override void UpdateIsNew()
        {
            if (!(Entity is WardrobeShortInfo wardrobe))
            {
                return;
            }
            
            _newIcon.SetActive(wardrobe.IsNew);
        }

        private void SetupBadge()
        {
            if (!(Entity is WardrobeShortInfo wardrobe) || !wardrobe.SeasonLevel.HasValue)
            {
                _badgeRequirementUI.gameObject.SetActive(false);
                return;
            }

            _badgeRequirementUI.gameObject.SetActive(true);
            _badgeRequirementUI.SetLevelRequirement(wardrobe.SeasonLevel.Value);
        }

        private void SetupPrice()
        {
            if (!_hasTier)
            {
                _priceHolder.SetActive(false);
                return;
            }
            
            switch (Entity)
            {
                case WardrobeShortInfo wardrobe:
                    _priceHolder.SetActive(true);
                    _priceHolder.SetCost(wardrobe.AssetOffer, IsOwned);
                    return;
                case WardrobeFullInfo wardrobe:
                    _priceHolder.SetActive(true);
                    _priceHolder.SetCost(wardrobe.AssetOffer, IsOwned);
                    return;
                default:
                    _priceHolder.SetActive(false);
                    return;
            }
        }

        protected virtual void SetupTier()
        {
            _hasTier = false;

            if (_tierBackgroundUI is null) return;

            AssetTierInfo tier = null;
            
            switch (Entity)
            {
                case WardrobeShortInfo wardrobe:
                    _hasTier = IsOwned || !(wardrobe.AssetTier == null && wardrobe.AssetOffer == null);
                    tier = wardrobe.AssetTier;
                    break;
                case WardrobeFullInfo wardrobe:
                    _hasTier = IsOwned || !(wardrobe.AssetTier == null && wardrobe.AssetOffer == null);
                    tier = wardrobe.AssetTier;
                    break;
            }
            
            _tierBackgroundUI.SetActive(_hasTier);
            
            if (!_hasTier || tier == null)
            {
                return;
            }
            
            _tierBackgroundUI.SetTier(tier.Id);
        }

        private void OnSelectButtonClicked()
        {
            _wardrobesInputHandler.Process(ItemSelected);

            void ItemSelected()
            {
                if (IsLoading)
                {
                    return;
                }
                
                OnItemSelected();
            }
        }

        private void OnBuyButtonClicked()
        {
            _buyBtnObj.SetActive(false);
            _confirmBtnObj.SetActive(true);
        }
        
        private void OnConfirmButtonClicked()
        {
            _confirmBtnObj.SetActive(false);
            PurchaseRequested?.Invoke(Entity);
        }

        public void UpdateOwnershipAnimated(bool isOwned)
        {
            var animate = !IsOwned && isOwned;

            IsOwned = isOwned;

            if (animate)
            {
                _priceHolder.AnimateOwnedIcon();
            }
        }
    }
}
