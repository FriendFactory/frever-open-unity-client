using System;
using System.Threading;
using Bridge;
using Bridge.Models.ClientServer.Invitation;
using Bridge.Services.UserProfile;
using Modules.CharacterManagement;
using TMPro;
using UIManaging.Localization;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.PopupSystem.Popups
{
    public class InviteRewardPopup : BasePopup<InviteRewardPopupConfiguration>
    {
        [Inject] private IBridge _bridge;
        [Inject] private CharacterManager _characterManager;
        [Inject] private InviteRewardPopupLocalization _localization;

        [SerializeField] private Button _closeButton;
        [Space]
        [SerializeField] private RawImage _inviterThumbnail;
        [SerializeField] private TMP_Text _header;
        [SerializeField] private TMP_Text _message;
        [SerializeField] private Button _claimButton;
        [Space] 
        [SerializeField] private TMP_Text _softCurrencyAmount;

        private Profile _inviterProfile;
        private CancellationTokenSource _tokenSource;

        private void OnEnable()
        {
            _closeButton.onClick.AddListener(OnCloseButtonClick);
            _claimButton.onClick.AddListener(OnClaimButtonClick);
        }

        private void OnDisable()
        {
            _closeButton.onClick.RemoveListener(OnCloseButtonClick);
            _claimButton.onClick.RemoveListener(OnClaimButtonClick);
        }

        protected override async void OnConfigure(InviteRewardPopupConfiguration configuration)
        {
            _tokenSource = new CancellationTokenSource();
            
            SetupTextSection(configuration.FromStarCreator, configuration.Reward);
            SetupRewards(configuration.Reward.SoftCurrency);
            var rewardResult = await _bridge.GetUnclaimedInviteeReward(_tokenSource.Token);
            if (rewardResult.IsError)
            {
                Debug.LogError(rewardResult.ErrorMessage);
                return;
            }
            if(rewardResult.IsRequestCanceled) return;

            var reward = rewardResult.Model;
           Debug.Log($"Soft currency reward: {reward.SoftCurrency}"); 
            var profileResult = await _bridge.GetProfile(reward.InviterGroupId, _tokenSource.Token);

            if (profileResult.IsError)
            {
                Debug.LogError(profileResult.ErrorMessage);
                return;
            }
            if(profileResult.IsRequestCanceled) return;

            _inviterProfile = profileResult.Profile;
            SetInviterThumbnail(_inviterProfile.MainCharacter, _tokenSource.Token);
        }

        protected override void OnHidden()
        {
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
        }

        private async void SetInviterThumbnail(CharacterInfo mainCharacterInfo, CancellationToken token)
        {
            var texture = await _characterManager.GetCharacterThumbnail(mainCharacterInfo, Resolution._128x128, token);
            _inviterThumbnail.texture = texture;
        }

        private void SetupTextSection(bool fromStarCreator, InviteeReward reward)
        {
            if (fromStarCreator)
            {
                SetupTextForCreatorInvite(reward);
                return;
            }
            
            SetupTextForFriendInvite(reward);
        }

        private void SetupTextForFriendInvite(InviteeReward reward)
        {
            _header.text = _localization.FriendInviteHeader;
            _message.text = string.Format(_localization.FriendInviteDescriptionFormat, reward.InviterNickName);
        }
        
        private void SetupTextForCreatorInvite(InviteeReward reward)
        {
            _header.text = string.Format(_localization.CreatorInviteHeaderFormat, reward.InviterNickName);
            _message.text = reward.WelcomeMessage;
        }

        private void SetupRewards(int softCurrency)
        {
            _softCurrencyAmount.text = softCurrency.ToString();
        }

        private void OnCloseButtonClick()
        {
            Hide(false);
        }

        private void OnClaimButtonClick()
        {
            _bridge.ClaimRewardForInvitedUser();
            _bridge.StartFollow(_inviterProfile.MainGroupId);
            
            Hide(true);
        }
        
    }
}