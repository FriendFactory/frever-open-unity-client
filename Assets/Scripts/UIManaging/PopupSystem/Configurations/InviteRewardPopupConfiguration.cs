using System;
using Bridge.Models.ClientServer.Invitation;
using Modules.DeepLinking;

namespace UIManaging.PopupSystem.Configurations
{
    public sealed class InviteRewardPopupConfiguration : PopupConfiguration
    {
        public InviteRewardPopupConfiguration(InviteeReward reward, Action<object> onClose)
            : base(PopupType.InviteRewardPopup, onClose)
        {
            Reward = reward;
            FromStarCreator = !string.IsNullOrEmpty(reward.WelcomeMessage);
        }

        public readonly bool FromStarCreator;
        public readonly InviteeReward Reward;
    }
}