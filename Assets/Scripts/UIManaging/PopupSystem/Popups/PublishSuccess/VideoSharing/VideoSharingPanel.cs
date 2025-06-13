using System;
using Common.Abstract;
using Common.UserBalance;
using Extensions;
using TMPro;
using UIManaging.Common.Buttons;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Tasks.RewardPopUp;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.PopupSystem.Popups.PublishSuccess.VideoSharing
{
    public sealed class VideoSharingPanel : BaseContextPanel<VideoSharingModel>
    {
        [SerializeField] private Button _shareToButton;
        [SerializeField] private LoadableButton _nativeShareButton;
        [SerializeField] private TMP_Text _nativeShareButtonLabel;
        [SerializeField] private TMP_Text _shareCountText;
        [Header("Flying Coins Animation")]
        [SerializeField] private UserBalanceView _userBalanceView;
        [SerializeField] private FlyingRewardsAnimationController _flyingRewardsAnimationController;
        [Header("L10N")] 
        [SerializeField] private PublishSuccessPopupLocalization _localization;

        [Inject] private LocalUserDataHolder _localUserDataHolder;

        private int SoftCurrencyReward => ContextData.SoftCurrencyReward;

        public event Action ShareToRequested;
        public event Action NativeShareRequested;

        public void ToggleLoading(bool isOn)
        {
            _nativeShareButton.ToggleLoading(isOn);
        }

        protected override void OnInitialized()
        {
            _shareToButton.onClick.AddListener(OnShareToRequested);
            _nativeShareButton.OnClicked.AddListener(OnNativeShareToRequested);
            
            var userBalanceModel = new StaticUserBalanceModel(_localUserDataHolder);
            _userBalanceView.Initialize(userBalanceModel);

            if (ContextData.ShareDailyLimitReached)
            {
                _shareCountText.SetActive(false);
                return;
            }

            UpdateShareCountText(ContextData.ShareCount);
            UpdateNativeShareButtonLabel(!ContextData.ShareDailyLimitReached);

            ContextData.ShareCountChanged += OnShareCountChanged;
        }

        protected override void BeforeCleanUp()
        {
            _userBalanceView.CleanUp();
            
            _shareToButton.onClick.RemoveListener(OnShareToRequested);
            _nativeShareButton.OnClicked.RemoveListener(OnNativeShareToRequested);

            ContextData.ShareCountChanged -= OnShareCountChanged;
        }

        private void OnShareCountChanged(int value)
        {
            UpdateShareCountText(value);
            UpdateNativeShareButtonLabel(false);
            
            PlaySoftCurrencyAnimation();
        }

        private void UpdateNativeShareButtonLabel(bool shareAvailable)
        {
            var text = shareAvailable
                ? string.Format(_localization.ShareAvailableButtonLabel, SoftCurrencyReward)
                : _localization.ShareUnavailableButtonLabel;

            _nativeShareButtonLabel.text = text;
        }

        private void UpdateShareCountText(int shareCount)
        {
            var shareLimit = ContextData.RewardedShareLimit;
            shareCount = Mathf.Clamp(shareCount, 0, shareLimit);
            var progress = $"{shareCount}/{shareLimit}";

            _shareCountText.text = string.Format(_localization.DailyVideoSharingProgress, progress);
        }

        private void PlaySoftCurrencyAnimation()
        {
            if (_userBalanceView.IsInitialized)
            {
                _userBalanceView.ContextData.CleanUp();
                _userBalanceView.CleanUp();
            }

            _flyingRewardsAnimationController.FirstElementReachedTarget += PlayUserBalanceAnimation;
            
            _flyingRewardsAnimationController.Play(true, false, false);

            void PlayUserBalanceAnimation()
            {
                _flyingRewardsAnimationController.FirstElementReachedTarget -= PlayUserBalanceAnimation;
            
                var userBalance = _localUserDataHolder.UserBalance;
                var fromSoft = userBalance.SoftCurrencyAmount;
                var toSoft = userBalance.SoftCurrencyAmount + SoftCurrencyReward;
                var hardAmount = _localUserDataHolder.UserBalance.HardCurrencyAmount;

                var args = new UserBalanceArgs(0f, 1f, fromSoft, toSoft, hardAmount, hardAmount);

                _userBalanceView.Initialize(new AnimatedUserBalanceModel(args));
            }
        }

        private void OnShareToRequested() => ShareToRequested?.Invoke();
        private void OnNativeShareToRequested() => NativeShareRequested?.Invoke();
    }
}