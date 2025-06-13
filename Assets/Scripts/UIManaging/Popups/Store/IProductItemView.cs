using System;
using Modules.InAppPurchasing;

namespace UIManaging.Popups.Store
{
    public interface IProductItemView
    {
        PurchasableProduct Product { get; }
        event Action<PurchasableProduct> Selected;
        void Initialize(PurchasableProduct product);
        void Show();
        void Hide();
    }

    public interface ICurrencyProductItemView : IProductItemView
    {
        event Action<PurchasableProduct> NotEnoughCurrencyForPurchase;
    }
}