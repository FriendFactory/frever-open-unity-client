using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using VoxelBusters.EssentialKit;

namespace Modules.InAppPurchasing
{
    internal interface IStoreProductsProvider
    {
        IBillingProduct[] Products { get; }
    }

    /// <summary>
    /// Provides real products taken by native plugin we use (Essential Kit).
    /// For Unity Editor it will provide test data, which you can setup under ProjectSettings/Billing Services
    /// </summary>
    [UsedImplicitly]
    internal sealed class StoreProductsProvider : IStoreProductsProvider
    {
        public IBillingProduct[] Products => BillingServices.Products;
    }

    /// <summary>
    /// Provides fake products to simulate working app on devices with bundle ids, which are not registered in our store(usually used for feature test builds)
    /// </summary>
    [UsedImplicitly]
    internal sealed class MockStoreProductsProvider : IStoreProductsProvider
    {
        public IBillingProduct[] Products 
        {
            get
            {
                if (_storeProducts != null) return _storeProducts;
                
                var mockList = Resources.Load<StoreProductList>("ScriptableObjects/Mock Store Products");
                if (mockList == null) throw new InvalidOperationException("Failed to load mock store products.");
                _storeProducts = mockList.StoreProducts.Cast<IBillingProduct>().ToArray();
                return _storeProducts;
            }
        }

        private IBillingProduct[] _storeProducts;

    }
}