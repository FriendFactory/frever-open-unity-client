namespace Modules.InAppPurchasing
{
    public sealed class SoftCurrencyProduct : PurchasableProduct
    {
        public long Id { get; set; }
        public override ProductType Type => ProductType.SoftCurrency;
    }
}
