using System.Collections.Generic;
using UnityEngine;

namespace Modules.InAppPurchasing
{
    [CreateAssetMenu(menuName = "Friend Factory/IAP/Mock Products List", order = 1, fileName = "Mock Store Products")]
    internal sealed class StoreProductList : ScriptableObject
    {
        public List<StoreProduct> StoreProducts;
    }
}