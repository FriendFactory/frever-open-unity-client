namespace Modules.InAppPurchasing
{
    public sealed class HardCurrencyProduct : PurchasableProduct, IPurchasableForRealMoney
    {
        public string OfferKey { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public override ProductType Type => ProductType.HardCurrency;
    }
}
