using DG.Tweening;
using Extensions;
using TMPro;
using UIManaging.Localization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.SeasonPage
{
    public class SeasonRewardPreviewPopup : MonoBehaviour
    {

        [SerializeField] private GameObject _softCurrencyContainer;
        [SerializeField] private GameObject _hardCurrencyContainer;
        [SerializeField] private GameObject _assetContainer;
        [FormerlySerializedAs("_unclaimedIcon")] [SerializeField] private GameObject _lockedIcon;
        [SerializeField] private GameObject _claimedIcon;
        [SerializeField] private TextMeshProUGUI _softCurrencyAmountText;
        [SerializeField] private TextMeshProUGUI _hardCurrencyAmountText;
        [SerializeField] private Image _assetImage;
        [SerializeField] private Image _levelIconImg;
        [SerializeField] private TextMeshProUGUI _levelNumberText;
        [SerializeField] private TextMeshProUGUI _levelLabelText;
        [SerializeField] private TextMeshProUGUI _purchaseStatusText;
        [SerializeField] private Button _tryItOnButton;
        [SerializeField] private Button _exploreButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private SeasonLevelPurchaseButton _purchaseButton;
        [SerializeField] private Sprite _levelIconUnlockedSprite;
        [SerializeField] private Sprite _levelIconLockedSprite;

        [Inject] private SeasonPageLocalization _localization;

        private SeasonRewardPreviewModel _model;
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            _tryItOnButton.onClick.AddListener(OnTryItOn);
            _exploreButton.onClick.AddListener(OnExplore);
            _closeButton.onClick.AddListener(Hide);
            _purchaseButton.OnLevelPurchased += Hide;
        }

        private void OnDisable()
        {
            _tryItOnButton.onClick.RemoveListener(OnTryItOn);
            _exploreButton.onClick.RemoveListener(OnExplore);
            _closeButton.onClick.RemoveListener(Hide);
            _purchaseButton.OnLevelPurchased -= Hide;
            
            ResetFadeAnimation();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Show(SeasonRewardPreviewModel model)
        {
            _model = model;

            _levelIconImg.sprite = _model.IsLocked ? _levelIconLockedSprite : _levelIconUnlockedSprite;
            
            if (model.TargetLevel <= model.CurrentLevel)
            {
                ShowClaimedReward();
            }
            else
            {
                ShowUnclaimedReward();
            }

            if (model.Reward.SoftCurrency.HasValue)
            {
                ShowSoftCurrency();
            } 
            else if (model.Reward.HardCurrency.HasValue)
            {
                ShowHardCurrency();
            }
            else
            {
                ShowAsset();
            }
            
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            _model.CancelLoading();
            _assetImage.sprite = null;
            gameObject.SetActive(false);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnTryItOn()
        {
            _model.TryAsset(Hide);
        }

        private void OnExplore()
        {
            _model.ExplorePremium();
            Hide();
        }
        
        private void ShowClaimedReward()
        {
            _levelNumberText.text = _model.TargetLevel.ToString();
            _levelLabelText.text = _model.IsLocked
                ? string.Format(_localization.RewardPreviewClaimedLabelLocked, _model.TargetLevel)
                : string.Format(_localization.RewardPreviewClaimedLabelUnlocked, _model.TargetLevel);
            _purchaseStatusText.text = _model.IsLocked 
                ?  _localization.RewardPreviewClaimedStatusLocked
                : _model.Reward.Asset == null
                    ? _localization.RewardPreviewClaimedStatusUnlockedReward
                    : _localization.RewardPreviewClaimedStatusUnlockedAsset;
            
            _tryItOnButton.SetActive(!_model.IsLocked && _model.Reward.Asset != null);
            _purchaseButton.SetActive(false);
            _exploreButton.SetActive(_model.IsLocked);
            
            _lockedIcon.SetActive(_model.IsLocked);
            _claimedIcon.SetActive(!_model.IsLocked);
        }

        private void ShowUnclaimedReward()
        {
            _levelNumberText.text = _model.TargetLevel.ToString();
            _levelLabelText.text = _localization.RewardPreviewUnclaimedLabel;
            _purchaseStatusText.text = _model.IsLocked
                ? _localization.RewardPreviewUnclaimedStatusLocked
                : string.Format(_localization.RewardPreviewUnclaimedStatusUnlocked, (_model.TargetLevel - _model.CurrentLevel).ToString());

            _purchaseButton.LevelAmount = _model.TargetLevel - _model.CurrentLevel;
            
            _tryItOnButton.SetActive(false);
            _purchaseButton.SetActive(!_model.IsLocked);
            _exploreButton.SetActive(_model.IsLocked);
            _lockedIcon.SetActive(_model.IsLocked);
            _claimedIcon.SetActive(false);
        }

        private void ShowSoftCurrency()
        {
            _softCurrencyContainer.SetActive(true);
            _hardCurrencyContainer.SetActive(false);
            _assetContainer.SetActive(false);
            _softCurrencyAmountText.text = _model.Reward.SoftCurrency.ToString();
        }

        private void ShowHardCurrency()
        {
            _softCurrencyContainer.SetActive(false);
            _hardCurrencyContainer.SetActive(true);
            _assetContainer.SetActive(false);
            _hardCurrencyAmountText.text = _model.Reward.HardCurrency.ToString();
        }

        private void ShowAsset()
        {
            _softCurrencyContainer.SetActive(false);
            _hardCurrencyContainer.SetActive(false);
            _assetContainer.SetActive(true);
            _assetImage.SetActive(false);
            _model.DownloadThumbnail(OnThumbnailLoaded);
        }
        
        private void OnThumbnailLoaded(Texture2D texture)
        {
            if (_assetImage.IsDestroyed()) return;

            var rect = new Rect(0f, 0f, texture.width, texture.height);
            var pivot = new Vector2(0.5f, 0.5f);
            _assetImage.sprite = Sprite.Create(texture, rect, pivot);
            _assetImage.SetActive(true);

            FadeIn();
        }

        private void FadeIn()
        {
            ResetFadeAnimation();
            _assetImage.DOFade(1f, 0.3f);
        }

        private void ResetFadeAnimation()
        {
            _assetImage.DOKill();
            _assetImage.SetAlpha(0);
        }
    }
}