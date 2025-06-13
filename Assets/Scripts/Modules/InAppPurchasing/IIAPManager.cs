using System;
using System.Threading.Tasks;

namespace Modules.InAppPurchasing
{
    public interface IIAPManager
    {
        event Action InitializeFinished;
        event Action PurchaseStarted;
        event Action PurchaseFailed;
        event Action PurchaseSuccess;
        event Action<PurchasableProduct> PurchaseConfirmed;
        event Action<SeasonPassProduct> PurchasedSeasonPass;

        void Initialize();
        void BuyProduct(IPurchasableForRealMoney id);
        void BuySoftCurrency(string id);
        void RestorePurchases();
        PurchasableProduct[] GetAvailableProducts();
        SeasonPassProduct GetSeasonPassProduct();
        Task<bool> ClaimFreeSeasonPass();
    }
}
