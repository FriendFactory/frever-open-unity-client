using Modules.InAppPurchasing;

namespace UIManaging.Popups.Store
{
    internal sealed class BuyGemsItemView : BuyCurrencyItemView<HardCurrencyProduct>
    {
        protected override void OnBuyClicked()
        {
            base.OnBuyClicked();
            IAPManager.BuyProduct(ContextData);
        }
    }
}
