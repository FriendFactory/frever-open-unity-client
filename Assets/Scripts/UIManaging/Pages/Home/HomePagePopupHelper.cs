using System;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Gamification;
using Bridge.Models.ClientServer.Invitation;
using Modules.Amplitude;
using Modules.AssetsStoraging.Core;
using Modules.DeepLinking;
using Modules.QuestManaging;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.Common.Seasons;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Tasks;
using UIManaging.Pages.Tasks.TaskRewardFlow;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UnityEngine;

namespace UIManaging.Pages.Home
{
    internal sealed class HomePagePopupHelper: IDisposable
    {
        private readonly IBridge _bridge;
        private readonly PopupManager _popupManager;
        private readonly LocalUserDataHolder _localUserData;
        private readonly RewardFlowManager _rewardManager;
        private readonly IInvitationLinkHandler _invitationLinkHandler;
        private readonly SnackBarHelper _snackBarHelper;
        private readonly PageManager _pageManager;
        private readonly FeedPopupHelper _feedPopupHelper;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        private string _nickname;
        private long _groupId;
        private int _softCurrency;
        private InviteeReward _inviteeReward;

        public HomePagePopupHelper(IBridge bridge, PopupManager popupManager, LocalUserDataHolder localUserData, 
            RewardFlowManager rewardManager, IInvitationLinkHandler invitationLinkHandler,
            SnackBarHelper snackBarHelper, PageManager pageManager, FeedPopupHelper feedPopupHelper)
        {
            _bridge = bridge;
            _popupManager = popupManager;
            _localUserData = localUserData;
            _rewardManager = rewardManager;
            _invitationLinkHandler = invitationLinkHandler;
            _snackBarHelper = snackBarHelper;
            _pageManager = pageManager;
            _feedPopupHelper = feedPopupHelper;
        }

        private bool ShowDeepLinkPopup => _invitationLinkHandler.InvitationLinkInfo != null;
        
        public async void ShowHomePagePopups()
        {
            if (ShowDeepLinkPopup) await _bridge.UseInvitationCode(_invitationLinkHandler.InvitationLinkInfo.InvitationGuid);
            
            if(await TryShowUnclaimedRewardPopup()) return;
            
            await _localUserData.UpdateBalance();
            
            if(_feedPopupHelper.TryShowSeasonStartPopup()) return; 

            if(!ShowDeepLinkPopup) return;
            
            await SetupInviteRewardPopup();
            ShowInviteRewardPopup();
        }
        
        private async Task<bool> TryShowUnclaimedRewardPopup()
        {
            var unclaimedRewards = await _bridge.ClaimPastRewards();
            if (unclaimedRewards.IsSuccess && unclaimedRewards.Model?.RewardCount == 0) return false;

            var config = new SeasonUnclaimedRewardsPopupConfiguration(unclaimedRewards.Model, OnUnclaimedRewardsClosed);
            _popupManager.PushPopupToQueue(config);
            
            return true;
        }

        private void OnUnclaimedRewardsClosed(object result)
        {
            if (!(result is ClaimPastRewardsResult reward)) return;
            
            _rewardManager.StartAnimation(0, reward.SoftCurrency, reward.HardCurrency);
            _rewardManager.FlowCompleted += OnUnclaimedAnimationFinished;
        }
        
        private async void OnUnclaimedAnimationFinished(RewardFlowResult _)
        {
            _rewardManager.FlowCompleted -= OnUnclaimedAnimationFinished;

            await Task.Delay(TimeSpan.FromSeconds(1));
            
            _feedPopupHelper.TryShowSeasonStartPopup();
        }

        private async Task SetupInviteRewardPopup()
        {
            var rewardResult = await _bridge.GetUnclaimedInviteeReward(_tokenSource.Token);
            if (rewardResult.IsError)
            {
                Debug.LogError(rewardResult.ErrorMessage);
                return;
            }
            if(rewardResult.IsRequestCanceled) return;

            _inviteeReward = rewardResult.Model;
            _rewardManager.Initialize(_localUserData.UserBalance, _localUserData.LevelingProgress.Xp);
            var config = new InviteRewardPopupConfiguration(_inviteeReward, OnInviteRewardPopupClosed);
            
            _popupManager.SetupPopup(config);
        }
        
        private void ShowInviteRewardPopup()
        {
            if (_invitationLinkHandler.InvitationLinkInfo is null) return;
            
            _popupManager.ShowPopup(PopupType.InviteRewardPopup);
        }

        private void OnInviteRewardPopupClosed(object result)
        {
            _invitationLinkHandler.Clear();
            var claimed = (bool) result ;
            if(!claimed) return;

            _rewardManager.FlowCompleted += ShowSnackbar;
            _rewardManager.StartAnimation(0, _softCurrency, 0);
        }

        private void ShowSnackbar(object _)
        {
            _snackBarHelper.ShowInviterFollowedSnackBar(_inviteeReward.InviterNickName, OnClick);

            void OnClick()
            {
                var args = new UserProfileArgs(_inviteeReward.InviterGroupId, _inviteeReward.InviterNickName);
                _pageManager.MoveNext(args);
            }
        }

        public void Dispose()
        {
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
        }
    }
}