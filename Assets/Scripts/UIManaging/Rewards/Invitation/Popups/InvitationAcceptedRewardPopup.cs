using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Services.UserProfile;
using Modules.CharacterManagement;
using Modules.Notifications;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Localization;
using UIManaging.PopupSystem.Popups;
using UIManaging.Rewards.Models;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Rewards.Invitation.Popups
{
    internal class InvitationAcceptedRewardPopup: BasePopup<InvitationAcceptedRewardPopupConfiguration>
    {
        [SerializeField] private TMP_Text _header;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private TMP_Text _softCurrency;
        [SerializeField] private UserPortrait _inviteeThumbnail;
        [Space]
        [SerializeField] private ClaimAndFollowButton _claimButton;
        [SerializeField] private Button _closeButton;
        
        [Inject] private IBridge _bridge;
        [Inject] private CharacterManager _characterManager;
        [Inject] private PageManager _pageManager;
        [Inject] private INotificationHandler _notificationHandler;
        [Inject] private InviteRewardPopupLocalization _localization;

        private InvitationAcceptedRewardModel _rewardModel;
        private CancellationTokenSource _tokenSource;

        protected override async void OnConfigure(InvitationAcceptedRewardPopupConfiguration configuration)
        {
            _rewardModel = configuration.RewardModel;
            _tokenSource = new CancellationTokenSource();

            UpdateTextFields(_rewardModel);
            await LoadInviteePortraitAsync(configuration.Profile, _tokenSource.Token);

            _rewardModel.OnRewardClaimed += OnRewardClaimed;

            _claimButton.Initialize(_rewardModel);
            _closeButton.onClick.AddListener(Hide);
        }

        protected override void OnHidden()
        {
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();

            _rewardModel.OnRewardClaimed -= Hide;
            
            _closeButton.onClick.RemoveListener(Hide);
        }

        private void OnRewardClaimed(InvitationAcceptedRewardModel rewardModel)
        {
            Hide();
            
            // we need to forcefully update notification models to receive updated IsClaimed status from backend
            _notificationHandler.ClearNotifications();

            var args = new HomePageArgs { RewardModel = rewardModel };
            _pageManager.MoveNext(args, false);
        }

        private void UpdateTextFields(InvitationAcceptedRewardModel rewardModel)
        {
            var header = string.Format(_localization.UserSignedUpTitleFormat, rewardModel.NickName);
            var description = string.Format(_localization.UserSignedUpDescriptionFormat, rewardModel.NickName);
            var softCurrency = rewardModel.SoftCurrency.ToString();

            _header.SetText(header);
            _description.SetText(description);
            _softCurrency.SetText(softCurrency);
        }

        private async Task LoadInviteePortraitAsync(Profile profile, CancellationToken token)
        {
            await _inviteeThumbnail.InitializeAsync(profile, Resolution._128x128, token);
            _inviteeThumbnail.ShowContent();
        }
    }
}