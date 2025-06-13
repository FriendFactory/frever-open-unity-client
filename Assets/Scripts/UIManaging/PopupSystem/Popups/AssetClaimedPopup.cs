using System;
using Modules.Sound;
using TMPro;
using UIManaging.Localization;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UIManaging.SnackBarSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class AssetClaimedPopup: BasePopup<AssetClaimedPopupConfiguration>
    {
        [Header("Asset")]
        [SerializeField] private TMP_Text _assetTierIconText;
        [SerializeField] private TMP_Text _assetTierText;
        [SerializeField] private Image _thumbnail;
        [SerializeField] private AspectRatioFitter _thumbnailFitter;
        [Header("Buttons")]
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _closeButton;

        [Inject] private SnackBarManager _snackBarManager;
        [Inject] private ISoundManager _soundManager;
        [Inject] private SeasonPageLocalization _localization;

        private Coroutine _playRewardSoundsRoutine;
        private ClaimPopupSoundsTrigger _soundsTrigger;

        private void Awake()
        {
            _confirmButton.onClick.AddListener(Close);
            _closeButton.onClick.AddListener(Close);
        }

        private void OnDestroy()
        {
            _confirmButton.onClick.RemoveListener(Close);
            _closeButton.onClick.RemoveListener(Close);

            if (_playRewardSoundsRoutine != null)
            {
                StopCoroutine(_playRewardSoundsRoutine);
            }
        }

        protected override void OnConfigure(AssetClaimedPopupConfiguration configuration)
        {
            var thumbnail = configuration.Thumbnail;
            _thumbnail.sprite = thumbnail;
            _thumbnail.preserveAspect = true;
            _thumbnailFitter.aspectRatio = thumbnail.texture.width / (float) thumbnail.texture.height;
            
            _assetTierIconText.text =  configuration.Level.ToString();
            _assetTierText.text = Configs.Title;
        }

        public override void Show()
        {
            base.Show();
            _soundsTrigger = new ClaimPopupSoundsTrigger(_soundManager);
            _soundsTrigger.PlayClaimPopupSound();
            
            _playRewardSoundsRoutine = StartCoroutine(_soundsTrigger.PlayRewardAnimationSounds(OnSoundsFinished));
        }

        private void Close()
        {
            Hide();

            var title = _localization.AssetClaimedSnackbarTitle;
            var description = _localization.AssetClaimedSnackbarDescription;
            var assetClaimedConfiguration = new AssetSnackBarConfiguration(title, description, _thumbnail.sprite);
            _snackBarManager.Show(assetClaimedConfiguration);
        }
    
        private void OnSoundsFinished()
        {
            _playRewardSoundsRoutine = null;
        }
    }
}