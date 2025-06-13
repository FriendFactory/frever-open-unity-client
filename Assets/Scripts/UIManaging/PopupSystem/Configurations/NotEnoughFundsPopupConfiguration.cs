using System;

namespace UIManaging.PopupSystem.Configurations
{
    public sealed class NotEnoughFundsPopupConfiguration : PopupConfiguration
    {
        public bool IsOnboarding;
    }
    
    public sealed class NotEnoughFundsWithBuyOptionPopupConfiguration : PopupConfiguration
    {
        public Action OnBuyClicked;
        public Action OnCancelClicked;
    }
}
