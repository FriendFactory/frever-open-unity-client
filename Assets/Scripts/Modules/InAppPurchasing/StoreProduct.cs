using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using VoxelBusters.EssentialKit;

namespace Modules.InAppPurchasing
{
    [Serializable]
    internal struct StoreProduct: IBillingProduct
    {
        [field: SerializeField]
        public string Id { get; set; }
        public string PlatformId { get; set; }
        public string LocalizedTitle { get; set; }
        public string LocalizedDescription { get; set; }
        [field: SerializeField]
        public string Price { get; set; }
        public string LocalizedPrice { get; set; }
        [field: SerializeField]
        public string PriceCurrencyCode { get; set; }
        public object Tag { get; set; }
        public bool IsAvailable { get; }
        public IEnumerable<BillingProductPayoutDefinition> Payouts { get; }
    }
}