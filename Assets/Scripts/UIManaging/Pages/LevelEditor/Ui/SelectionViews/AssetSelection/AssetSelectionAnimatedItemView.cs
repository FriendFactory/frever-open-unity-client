using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Bridge.Models.Common;
using DG.Tweening;
using Extensions;
using Modules.Amplitude;
using JetBrains.Annotations;
using TMPro;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection
{
    public abstract class AssetSelectionAnimatedItemView : AssetSelectionItemView
    {
        private const float ANIMATION_DURATION = 0.2f;

        [Space]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TMP_FontAsset _selectedFont;
        [SerializeField] private TMP_FontAsset _unselectedFont;
        [Space]
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private Image _priceIcon;
        [SerializeField] private Sprite _softCurrencyIcon;
        [SerializeField] private Sprite _hardCurrencyIcon;
        [SerializeField] private Sprite _purchasedIcon;
        [Space]
        [SerializeField] private TMP_Text _levelText;
        [Space]
        [SerializeField] private RectTransform _animatedRect;
        [FormerlySerializedAs("_shrinkDeltaSize")] [SerializeField]
        private float _shrinkDeltaSizeKoef = 0.88f;
        private Vector2 _originalDeltaSize;
        private Vector2 _targetDeltaSize;

        private LocalUserDataHolder _userData;
        private AmplitudeManager _amplitudeManager;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        protected abstract string TitleDisplayText { get; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        private void Construct(LocalUserDataHolder userData, AmplitudeManager amplitudeManager)
        {
            _userData = userData;
            _amplitudeManager = amplitudeManager;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void Awake()
        {
            _originalDeltaSize = _animatedRect.sizeDelta;
            base.Awake();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        [SuppressMessage("ReSharper", "SwitchStatementHandlesSomeKnownEnumValuesWithDefault")]
        protected override void OnInitialized()
        {
            base.OnInitialized();

            _titleText.text = TitleDisplayText;

            if (_amplitudeManager.IsShoppingCartFeatureEnabled())
            {
                var entity = ContextData.RepresentedObject;
                var modelType = entity.GetModelType();
                switch (modelType)
                {
                    case DbModelType.BodyAnimation:
                    case DbModelType.SetLocation:
                    case DbModelType.Vfx:
                    case DbModelType.CameraFilter:
                        ShowPrice(entity);
                        ShowLevelBadge((IMinLevelRequirable) entity);
                        break;
                    default:
                        DisablePrice();
                        break;
                }    
            }
            else
            {
                DisablePrice();
            }

            RefreshSize();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void ShowLevelBadge(IMinLevelRequirable asset)
        {
            var level = asset.SeasonLevel;
            if (level.HasValue)
            {
                _levelText.text = level.Value.ToString();
                _levelText.transform.parent.SetActive(true);
            }
            else
            {
                _levelText.transform.parent.SetActive(false);
            }
        }

        private void ShowPrice(IEntity entity)
        {
            if (ContextData.IsPurchased)
            {
                ShowPurchasedBadge();
            }
            else
            {
                ShowPriceBadge((IPurchasable) entity);
            }
        }

        private void ShowPurchasedBadge()
        {
            _priceText.gameObject.SetActive(false);
            _priceIcon.gameObject.SetActive(true);
            _priceIcon.sprite = _purchasedIcon;
        }

        private void ShowPriceBadge(IPurchasable purchasable)
        {
            if (purchasable.AssetOffer != null)
            {
                ShowCurrency(purchasable);
            }
            else
            {
                ShowFreeBadge();
            }
        }

        private void ShowCurrency(IPurchasable purchasable)
        {
            _priceText.gameObject.SetActive(true);
            _priceIcon.gameObject.SetActive(true);

            var assetOffer = purchasable.AssetOffer;
            if (assetOffer.AssetOfferHardCurrencyPrice != null)
            {
                _priceText.text = assetOffer.AssetOfferHardCurrencyPrice.ToString();
                _priceIcon.sprite = _hardCurrencyIcon;
            }
            else
            {
                _priceText.text = assetOffer.AssetOfferSoftCurrencyPrice.ToString();
                _priceIcon.sprite = _softCurrencyIcon;
            }
        }

        private void ShowFreeBadge()
        {
            _priceText.gameObject.SetActive(true);
            _priceIcon.gameObject.SetActive(false);
            _priceText.text = "Free";
        }

        private void DisablePrice()
        {
            _priceIcon.gameObject.SetActive(false);
            _priceText.gameObject.SetActive(false);
        }

        private void RefreshSize()
        {
            _animatedRect.sizeDelta = ContextData.IsSelected ? _shrinkDeltaSizeKoef * _originalDeltaSize : _originalDeltaSize;
        }

        protected override void RefreshSelectionGameObjects()
        {
            base.RefreshSelectionGameObjects();

            _titleText.font = ContextData.IsSelected ? _selectedFont : _unselectedFont;

            if (ContextData.IsSelected)
            {
                DoShrink();
            }
            else
            {
                DoGrow();
            }
        }

        private void DoShrink()
        {
            if (IsDestroyed || !gameObject.activeInHierarchy || _animatedRect == null) return;
            
            _targetDeltaSize = _shrinkDeltaSizeKoef * _originalDeltaSize;
            _animatedRect.DOKill();
            _animatedRect.DOSizeDelta(_targetDeltaSize, ANIMATION_DURATION);
        }

        private void DoGrow()
        {
            if (IsDestroyed || !gameObject.activeInHierarchy || _animatedRect == null) return;
            
            _targetDeltaSize = _shrinkDeltaSizeKoef * _originalDeltaSize;
            _animatedRect?.DOKill();
            _animatedRect?.DOSizeDelta(_originalDeltaSize, ANIMATION_DURATION).SetUpdate(true);
        }

        protected override void OnDisable()
        {
            _animatedRect.DOKill();
            _animatedRect.sizeDelta = _targetDeltaSize;
            base.OnDisable();
        }
    }
}