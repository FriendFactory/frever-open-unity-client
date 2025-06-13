using System;
using Bridge.Services.UserProfile;
using UIManaging.PopupSystem.Configurations;
using UIManaging.Rewards.Models;

namespace UIManaging.Rewards.Invitation.Popups
{
    public class InvitationAcceptedRewardPopupConfiguration : PopupConfiguration 
    {
        public InvitationAcceptedRewardModel RewardModel { get; }
        public Profile Profile { get; }

        public InvitationAcceptedRewardPopupConfiguration(InvitationAcceptedRewardModel rewardModel, Profile profile,
            Action<object> onClose = null, string title = "") : base(PopupType.InvitationAcceptedReward, onClose, title)
        {
            RewardModel = rewardModel;
            Profile = profile;
        }
    }
}