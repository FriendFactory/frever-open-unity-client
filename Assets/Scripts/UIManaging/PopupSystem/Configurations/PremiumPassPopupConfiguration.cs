using System;

namespace UIManaging.PopupSystem.Configurations
{
    public sealed class PremiumPassPopupConfiguration: PopupConfiguration
    {
        public Action OnSuccessfulPassPurchase { get; set; }
    }
}