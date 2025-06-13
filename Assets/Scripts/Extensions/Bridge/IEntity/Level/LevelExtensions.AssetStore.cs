using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.StartPack.UserAssets;
using Bridge.Models.Common;
using Models;

namespace Extensions
{
    public static partial class LevelExtensions
    {
        public static List<IPurchasable> GetStoreAssets(this Level level, PurchasedAssetsData purchasedAssets)
        {
            var list = new List<IPurchasable>();

            foreach (var @event in level.Event)
            {
                list.AddRange(@event.GetStoreAssets(purchasedAssets));
            }

            return list.DistinctBy(purchasable => purchasable.AssetOffer.AssetId).ToList();
        }
    }
}