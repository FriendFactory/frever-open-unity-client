using System;
using System.Diagnostics.CodeAnalysis;
using Bridge.Models.ClientServer.StartPack.UserAssets;

namespace Extensions
{
    public static class PurchasedAssetsDataExtension
    {
        [SuppressMessage("ReSharper", "SwitchStatementHandlesSomeKnownEnumValuesWithDefault")]
        public static long[] GetPurchasedAssetsByType(this PurchasedAssetsData assetsData, DbModelType type)
        {
            var purchasedAssetIds = GetPurchasedAssetsByTypeInternal(assetsData, type);
            return purchasedAssetIds ?? Array.Empty<long>();
        }
        
        private static long[] GetPurchasedAssetsByTypeInternal(PurchasedAssetsData assetsData, DbModelType type)
        {
            switch (type)
            {
                case DbModelType.SetLocation:
                    return assetsData.SetLocations ?? Array.Empty<long>();
                case DbModelType.CameraFilter:
                    return assetsData.CameraFilters ?? Array.Empty<long>();
                case DbModelType.Vfx:
                    return assetsData.Vfxs ?? Array.Empty<long>();
                case DbModelType.VoiceFilter:
                    return assetsData.VoiceFilters ?? Array.Empty<long>();
                case DbModelType.BodyAnimation:
                    return assetsData.BodyAnimations ?? Array.Empty<long>();
                default:
                    return Array.Empty<long>();
            }
        }
    }
}