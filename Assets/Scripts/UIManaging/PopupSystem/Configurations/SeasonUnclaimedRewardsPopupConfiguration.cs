using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Gamification;
using Bridge.Results;

namespace UIManaging.PopupSystem.Configurations
{
    public class SeasonUnclaimedRewardsPopupConfiguration : PopupConfiguration
    {
        public ClaimPastRewardsResult Rewards { get; }

        public SeasonUnclaimedRewardsPopupConfiguration(ClaimPastRewardsResult rewards, Action<object> onClose = null, string title = "")
            : base(PopupType.SeasonUnclaimedRewards, onClose, title)
        {
            Rewards = rewards;
        }
    }
}