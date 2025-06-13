using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.AssetStore;
using JetBrains.Annotations;
using UnityEngine;
using VoxelBusters.CoreLibrary;
using VoxelBusters.EssentialKit;

namespace Modules.InAppPurchasing
{
    [UsedImplicitly]
    internal sealed class IAPManager : IIAPManager
    {
        private const string PENDING_ORDER_ID_KEY = "PendingOrderID";
        
        private bool _isInitialized;
        private Guid _pendingOrderId;
        private readonly IBridge _bridge;
        private readonly IStoreProductsProvider _storeProductsProvider;
        private readonly List<PurchasableProduct> _purchasableProducts = new List<PurchasableProduct>();

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action InitializeFinished;
        public event Action PurchaseStarted;
        public event Action PurchaseFailed;
        public event Action PurchaseSuccess;
        public event Action<PurchasableProduct> PurchaseConfirmed;
        public event Action<SeasonPassProduct> PurchasedSeasonPass;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private IBillingProduct[] Products => _storeProductsProvider.Products;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public IAPManager(IBridge bridge, IStoreProductsProvider storeProductsProvider)
        {
            _bridge = bridge;
            _storeProductsProvider = storeProductsProvider;
            BillingServices.OnInitializeStoreComplete += OnInitializeStoreComplete;
            BillingServices.OnTransactionStateChange += OnTransactionStateChange;
            BillingServices.OnRestorePurchasesComplete += OnRestorePurchasesComplete;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Initialize()
        {
            if (_isInitialized) return;
            
            BillingServices.InitializeStore();
        }

        public async void BuyProduct(IPurchasableForRealMoney product)
        {
            if (!_isInitialized || !BillingServices.CanMakePayments())
            {
                Debug.LogError("-- User was not allowed to make payments");
                return;
            }
            
            var initPurchasing = await InitPurchasing(product);
            if (!initPurchasing)
            {
                return;
            }

            if (string.IsNullOrEmpty(product.ReferenceId))
            {
                Debug.LogError($"Can't find ReferenceID for the product. Purchase can't be performed.");
                return;
            }

            PlayerPrefs.SetString(PENDING_ORDER_ID_KEY, _pendingOrderId.ToString());
            PlayerPrefs.Save();

            var storeProduct = Products.First(x=> x.Id == product.ReferenceId);
            BillingServices.BuyProduct(storeProduct);
            PurchaseStarted?.Invoke();
        }

        public async void BuySoftCurrency(string id)
        {
            var purchasedProduct = GetPurchasableProduct<SoftCurrencyProduct>(id);
            var result = await _bridge.ExchangeHardCurrency(purchasedProduct.Id);
            if (result.IsSuccess)
            {
                PurchaseConfirmed?.Invoke(purchasedProduct);
            }
            else if (result.IsError)
            {
                Debug.LogError($"Failed buy soft currency.  Reason:{result.ErrorMessage}");
            }
        }

        public void RestorePurchases()
        {
            BillingServices.RestorePurchases();
        }

        public PurchasableProduct[] GetAvailableProducts()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Billing service is not initialized");
            }

            return _purchasableProducts.ToArray();
        }

        public SeasonPassProduct GetSeasonPassProduct()
        {
            return _purchasableProducts.FirstOrDefault(x => x.Type == ProductType.SeasonPass) as SeasonPassProduct;
        }

        public async Task<bool> ClaimFreeSeasonPass()
        {
            var product = GetSeasonPassProduct();
            if (product is not { })
            {
                Debug.LogError($"Season Pass is not found. Skip claiming it");
                return false;
            }

            var initPurchasingSuccess = await InitPurchasing(product);
            if (!initPurchasingSuccess)
            {
                return false;
            }
            
            if (!product.IsFree)
            {
                Debug.LogWarning($"Season Pass is claimed as a free product, but it's price is {product.PriceText}");
            }
            
            var validateResult = await _bridge.CompletePurchasingInAppProduct(_pendingOrderId, "Free");
            if (validateResult.IsSuccess)
            {
                PurchasedSeasonPass?.Invoke(product);
                return true;
            }
            else
            {
                PurchaseFailed?.Invoke();
                return false;
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private async void OnInitializeStoreComplete(BillingServicesInitializeStoreResult result, Error error)
        {
            if (error != null)
            {
                return;
            }

            await SetupProducts();
            
            CheckAndHandleUnfinishedTransactions();
            _isInitialized = true;
            InitializeFinished?.Invoke();
        }
        private void OnRestorePurchasesComplete(BillingServicesRestorePurchasesResult result, Error error)
        {
            if (error == null)
            {
                var transactions = result.Transactions;
                Debug.Log("Request to restore purchases finished successfully.");
                Debug.Log("Total restored products: " + transactions.Length);

                foreach (var transaction in transactions)
                {
                    Debug.Log($"[{transaction}]: {transaction.Payment.ProductId}");
                }
            }
            else
            {
                Debug.Log("Request to restore purchases failed with error. Error: " +  error);
            }
        }

        private void OnTransactionStateChange(BillingServicesTransactionStateChangeResult result)
        {
            var transactions = result.Transactions;

            HandleNewTransations(transactions);
        }

        private void HandleNewTransations(IBillingTransaction[] transactions)
        {
            foreach (var transaction in transactions)
            {
                switch (transaction.TransactionState)
                {
                    case BillingTransactionState.Purchased:
                        PurchaseSuccess?.Invoke();
                        //Note: transaction.ReceiptVerificationState also needs to be considered for avoiding fraud transactions
                        Debug.Log($"Buy product with id:{transaction.Payment.ProductId} finished successfully with verification state {transaction.ReceiptVerificationState}.");
                        Debug.Log($"Purchase receipt: {transaction.Receipt}");
                        CompletePurchase(transaction);
                        break;
                    case BillingTransactionState.Failed:
                        Debug.Log($"Buy product with id:{transaction.Payment.ProductId} failed with error. Error: {transaction.Error}");
                        PurchaseFailed?.Invoke();
                        break;
                    case BillingTransactionState.Purchasing:
                        break;
                    case BillingTransactionState.Restored:
                        break;
                    case BillingTransactionState.Deferred:
                        Debug.Log($"Product with id:{transaction.Payment.ProductId} was Deferred");
                        break;
                    case BillingTransactionState.Refunded:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private async Task SetupProducts()
        {
            var productOffersResponse = await _bridge.GetProductOffers();
            if (productOffersResponse.IsError)
            {
                Debug.LogError($"Could not find product offers. [Reason]: {productOffersResponse.ErrorMessage}");
                return;
            }

            var productOffers = productOffersResponse.Model;
            var products = new List<PurchasableProduct>();
        
            AddSeasonPass(productOffers, products);
            AddHardCurrencyProducts(productOffers, products);
            AddSoftCurrencyProducts(productOffers, products);

            _purchasableProducts.AddRange(products);
        }

        private static void AddSoftCurrencyProducts(AvailableOffers productOffers, ICollection<PurchasableProduct> products)
        {
            foreach (var product in productOffers.CurrencyExchange)
            {
                if (string.IsNullOrEmpty(product.Title)) continue;

                var appProduct = new SoftCurrencyProduct
                {
                    Id = product.Id, ReferenceId = product.Title, PriceText = $"{product.HardCurrencyRequired}",
                    Amount = product.SoftCurrencyGiven
                };
                products.Add(appProduct);
            }
        }

        private void AddHardCurrencyProducts(AvailableOffers productOffers, ICollection<PurchasableProduct> products)
        {
            foreach (var product in productOffers.HardCurrencyOffers)
            {
                var storeProduct = GetStoreProduct(product);
                if (storeProduct == null) continue;

                var productOfferDetails = product.Details.First();
                var currencyAmount = productOfferDetails.HardCurrency ?? productOfferDetails.SoftCurrency;

                var appProduct = new HardCurrencyProduct
                {
                    OfferKey = product.OfferKey, ReferenceId = storeProduct.Id,
                    PriceText = $"{storeProduct.PriceCurrencyCode} {storeProduct.Price}",
                    Amount = currencyAmount,
                    Currency = storeProduct.PriceCurrencyCode,
                    Price = decimal.Parse(storeProduct.Price, System.Globalization.CultureInfo.InvariantCulture)
                };
                products.Add(appProduct);
            }
        }

        private void AddSeasonPass(AvailableOffers productOffers, List<PurchasableProduct> products)
        {
            var seasonPass = productOffers.InAppProducts?.FirstOrDefault(x => x.Offer.IsSeasonPass);
            if (seasonPass == null) return;
            var seasonPassProduct = new SeasonPassProduct
            {
                OfferKey = seasonPass.Offer.OfferKey,
                ProductOffer = seasonPass.Offer,
            };
            
            var storeProduct = GetStoreProduct(seasonPass.Offer);
            if (storeProduct is not null)
            {
                seasonPassProduct.ReferenceId = storeProduct.Id;
                seasonPassProduct.PriceText = $"{storeProduct.PriceCurrencyCode} {storeProduct.Price}";
                seasonPassProduct.Price = decimal.Parse(storeProduct.Price, System.Globalization.CultureInfo.InvariantCulture);
                seasonPassProduct.Currency = storeProduct.PriceCurrencyCode;
            }
            products.Add(seasonPassProduct);
        }

        private IBillingProduct GetStoreProduct(InAppProductOffer offer)
        {
            string key;
            #if UNITY_IOS
            key = offer.AppStoreProductRef;
            #else
            key = offer.PlayMarketProductRef;
            #endif
            return Products.FirstOrDefault(x => string.Equals(x.Id, key, StringComparison.OrdinalIgnoreCase));
        }

        private async void CompletePurchase(IBillingTransaction transaction)
        {
            var validateResult = await _bridge.CompletePurchasingInAppProduct(_pendingOrderId, transaction.Receipt);
            if (validateResult.IsSuccess)
            {
                var product = FindPurchasableProduct(transaction.Payment.ProductId);
                PurchaseConfirmed?.Invoke(product);
                var seasonPassProduct = GetSeasonPassProduct(); 
                var isSeasonPass = product.ReferenceId == seasonPassProduct?.ReferenceId;
                if (isSeasonPass)
                {
                    PurchasedSeasonPass?.Invoke(seasonPassProduct);
                }
            }
            else
            {
                PurchaseFailed?.Invoke();
            }
            PlayerPrefs.DeleteKey(PENDING_ORDER_ID_KEY);
        }

        private PurchasableProduct FindPurchasableProduct(string productId)
        {
            return _purchasableProducts.First(x => x.ReferenceId == productId);
        }
        
        private T GetPurchasableProduct<T>(string productId) where T: PurchasableProduct
        {
            return (T)FindPurchasableProduct(productId);
        }
        
        private void CheckAndHandleUnfinishedTransactions()
        {
            if (!PlayerPrefs.HasKey(PENDING_ORDER_ID_KEY))
            {
                return;
            }
            _pendingOrderId = new Guid(PlayerPrefs.GetString(PENDING_ORDER_ID_KEY));
            var transactions = BillingServices.GetTransactions();
            HandleNewTransations(transactions);
        }

        private async Task<bool> InitPurchasing(IPurchasableForRealMoney product)
        {
            var purchaseProductResult = await _bridge.InitPurchasingInAppProduct(product.OfferKey, product.Currency, product.Price);
            if (purchaseProductResult.IsError)
            {
                Debug.LogError($"Failed to initialize product purchase.  Reason:{purchaseProductResult.ErrorMessage}");
                return false;
            }

            _pendingOrderId = purchaseProductResult.PendingOrderId;
            return true;
        }
    }
}
