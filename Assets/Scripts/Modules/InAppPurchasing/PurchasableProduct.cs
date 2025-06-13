namespace Modules.InAppPurchasing
{
    public abstract class PurchasableProduct
    {
        public int? Amount { get; set; }
        public string PriceText { get; set; }
        public string ReferenceId { get; set; }
        public abstract ProductType Type { get; }
    }

    public interface IPurchasableForRealMoney
    {
        string OfferKey { get; }
        string ReferenceId { get; }
        decimal Price { get; }
        string Currency { get; }
    }
}
