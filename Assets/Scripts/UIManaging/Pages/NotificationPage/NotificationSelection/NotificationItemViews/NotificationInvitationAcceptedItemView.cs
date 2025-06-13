using System.Threading;
using Bridge;
using Modules.Notifications.NotificationItemModels;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.Rewards.Invitation.Popups;
using UIManaging.Rewards.Models;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews
{
    internal class NotificationInvitationAcceptedItemView : NotificationItemView<NotificationInvitationAcceptedModel>
    {
        [Space] [SerializeField] private Button _claimButton;
        [SerializeField] private TMP_Text _claimButtonLabel;

        [Inject] private PageManager _pageManager;
        [Inject] private PopupManager _popupManager;
        [Inject] private IBridge _bridge;
        [Inject] private SnackBarHelper _snackbarHelper;

        private CancellationTokenSource _tokenSource;

        protected override string Description => string.Format(_localization.InvitationAcceptedFormat, ContextData.AcceptedBy.Nickname);

        protected override void OnInitialized()
        {
            base.OnInitialized();

            _tokenSource = new CancellationTokenSource();
            
            UpdateClaimButtonState(!ContextData.IsClaimed);

            _claimButton.onClick.AddListener(ShowClaimRewardPopup);
        }

        protected override void BeforeCleanup()
        {
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();

            _claimButton.onClick.RemoveListener(ShowClaimRewardPopup);
        }

        private void UpdateClaimButtonState(bool interactable)
        {
            _claimButton.interactable = interactable;
            _claimButtonLabel.text = interactable 
                ? _localization.ClaimUserInviteRewardButton 
                : _localization.UserInviteRewardClaimedButton;
        }

        private async void ShowClaimRewardPopup()
        {
            var rewardModel = new InvitationAcceptedRewardModel(ContextData.AcceptedBy.Id, ContextData.AcceptedBy.Nickname, ContextData.SoftCurrency);
            var result = await _bridge.GetProfile(ContextData.GroupId, _tokenSource.Token);
            
            if (result.IsRequestCanceled) return;

            if (result.IsError && result.Profile is null)
            {
                HandleBlockedOrDeletedUser(rewardModel);
                return;
            }
            
            var configuration = new InvitationAcceptedRewardPopupConfiguration(rewardModel, result.Profile);

            rewardModel.OnRewardClaimed += OnRewardClaimed;
            
            _popupManager.SetupPopup(configuration);
            _popupManager.ShowPopup(PopupType.InvitationAcceptedReward);
            
        }

        private void OnRewardClaimed(InvitationAcceptedRewardModel rewardModel)
        {
            rewardModel.OnRewardClaimed -= OnRewardClaimed;

            UpdateClaimButtonState(false);
        }

        private async void HandleBlockedOrDeletedUser(InvitationAcceptedRewardModel rewardModel)
        {
            var claimResult = await _bridge.ClaimRewardFromInvitedUser(ContextData.GroupId);
            if (claimResult.IsError)
            {
                Debug.LogError(claimResult.ErrorMessage);
                return;
            }

            if (claimResult.IsRequestCanceled) return;

            _snackbarHelper.ShowSuccessDarkSnackBar(string.Format(_localization.UserDoesNotExistSnackbarMessage, ContextData.AcceptedBy.Nickname));

            UpdateClaimButtonState(false);
            
            var args = new HomePageArgs { RewardModel = rewardModel };
            _pageManager.MoveNext(args);
        }
    }
}