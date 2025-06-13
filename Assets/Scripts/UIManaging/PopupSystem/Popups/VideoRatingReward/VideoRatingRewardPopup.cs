using System;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.VideoServer;
using Coffee.UIExtensions;
using TMPro;
using UIManaging.Localization;
using UIManaging.Pages.Common.VideoRating;
using UIManaging.Pages.Tasks.TaskRewardFlow;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.PopupSystem.Popups.VideoRatingReward
{
    public class VideoRatingRewardPopup : BasePopup<VideoRatingRewardPopupConfiguration>
    {
        [Inject] private IBridge _bridge;
        [Inject] private RatingFeedPageLocalization _localization;

        [SerializeField] private Button _claimButton;
        [SerializeField] private TMP_Text _softCurrencyAmount;
        [SerializeField] private VideoRatingRewardTierPanel _ratingTierPanel;
        [Header("VFX")]
        [SerializeField] private FlyingCoinsAnimationController _flyingCoinsAnimationController;
        [SerializeField] private float _confettiDelay = 0.25f;
        [SerializeField] private UIParticle _confettiVfx;
            
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            _claimButton.onClick.AddListener(OnClaimButtonClick);
        }

        private void OnDisable()
        {
            _claimButton.onClick.RemoveListener(OnClaimButtonClick);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnConfigure(VideoRatingRewardPopupConfiguration config)
        {
            SetupRewards(config.RatingResult);

            _flyingCoinsAnimationController.Initialize(new RewardModel(Configs.RatingResult.SoftCurrency));

            var tierModel = new VideoRatingTierModel(config.RatingResult.Rating);

            _ratingTierPanel.Initialize(tierModel);

            if (tierModel.Tier == VideoRatingRewardTier.Gold)
            {
                PlayConfettiAnimation();
            }
        }

        protected override void OnHidden()
        {
            base.OnHidden();
            
            _flyingCoinsAnimationController.CleanUp();
            _ratingTierPanel.CleanUp();
            
            _confettiVfx.StopEmission();
            _confettiVfx.Stop();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SetupRewards(RatingResult ratingResult)
        {
            _softCurrencyAmount.text = ratingResult.SoftCurrency.ToString();
        }

        private async void OnClaimButtonClick()
        {
            _claimButton.interactable = false;
            
            var result = await _bridge.ClaimRatedVideoReward(Configs.VideoId);
            
            _claimButton.interactable = true;
            
            _claimButton.onClick.RemoveListener(OnClaimButtonClick);
            
            await _flyingCoinsAnimationController.PlayAnimationAsync();

            if (result.IsSuccess)
            {
                Configs.RatingResult.IsRewardAvailable = false;
            }
            else if (result.IsError)
            {
                Debug.LogError(result.ErrorMessage);
            }
            
            Hide(true);
        }

        private async void PlayConfettiAnimation()
        {
            await Task.Delay(TimeSpan.FromSeconds(_confettiDelay));
            
            _confettiVfx.StartEmission();
            _confettiVfx.Play();
        }
    }
}