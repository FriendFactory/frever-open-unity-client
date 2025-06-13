using Bridge.Models.ClientServer.AssetStore;

namespace Modules.InAppPurchasing
{
    public sealed class SeasonPassProduct : PurchasableProduct, IPurchasableForRealMoney
    {
        public override ProductType Type => ProductType.SeasonPass;
        public string OfferKey { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public InAppProductOffer ProductOffer { get; set; }
        public bool IsFree => Price == 0m;
    }
}