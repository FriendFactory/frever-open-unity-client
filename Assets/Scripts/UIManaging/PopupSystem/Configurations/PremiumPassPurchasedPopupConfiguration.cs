using System;

namespace UIManaging.PopupSystem.Configurations
{
    public sealed class PremiumPassPurchasedPopupConfiguration : PopupConfiguration
    {
        public bool ShowSeasonRewardsButton;
        public Action OnSeasonRewardsButtonClicked;
        public Action OnExitClicked;
    }
}