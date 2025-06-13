using System;
using Modules.InAppPurchasing;
using UnityEngine;

namespace UIManaging.Popups.Store.SuccessWindow
{
    [Serializable]
    public class PurchaseSuccessWindowContent
    {
        public ProductType Type = ProductType.SoftCurrency;
        public Sprite BigCurrencyIcon;
        public Sprite SmallCurrencyIcon;
    }
}
