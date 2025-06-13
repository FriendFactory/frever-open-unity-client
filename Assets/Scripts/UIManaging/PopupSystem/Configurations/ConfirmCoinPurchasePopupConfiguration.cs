using System;

namespace UIManaging.PopupSystem.Configurations
{
    public class ConfirmCoinPurchasePopupConfiguration : PopupConfiguration
    {
        public Action OnConfirm;
        public string SoftCurrencyAmount { get; set; }
        public string HardCurrencyCost { get; set; }
    }
}
