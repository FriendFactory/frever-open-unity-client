using System.Collections.Generic;
using System.Linq;
using Bridge.Models;
using Bridge.Models.ClientServer.Assets;

namespace Extensions
{
    public static class WardrobeExtensions
    {
        public static List<string> GetBundleWithDependencies(this UmaBundleFullInfo umaBundle)
        {
            var bundles = new List<string> { umaBundle.Name };
            if (umaBundle.DependentUmaBundles is { Count: > 0 })
            {
                bundles.AddRange(umaBundle.DependentUmaBundles.Select(x => x.Name));
            }

            return bundles;
        }

        public static string GetSlotName(this WardrobeFullInfo wardrobe)
        {
            return wardrobe.GetUmaAsset()?.SlotName;
        }
        
        public static UmaAssetInfo GetUmaAsset(this WardrobeFullInfo wardrobe)
        {
            return wardrobe.UmaBundle.UmaAssets.FirstOrDefault(x => x.SlotId != null && x.SlotId != 0);
        }

        public static WardrobeFullInfo[] GetWardrobesForGender(this OutfitFullInfo outfitFullInfo, long genderId)
        {
            return outfitFullInfo.Wardrobes.Where(x => outfitFullInfo.GenderWardrobes[genderId].Contains(x.Id))
                                 .ToArray();
        }

        public static WardrobeShortInfo ToShortInfo(this WardrobeFullInfo fullInfo)
        {
            return new WardrobeShortInfo
            {
                Id = fullInfo.Id,
                AssetOffer = fullInfo.AssetOffer,
                AssetTier = fullInfo.AssetTier,
                Name = fullInfo.Name,
                SeasonLevel = fullInfo.SeasonLevel,
                WardrobeCategoryId = fullInfo.CategoryId,
                Files = fullInfo.Files
            };
        }

        public static bool IsHair(this WardrobeFullInfo fullInfo)
        {
            return fullInfo.CategoryId == ServerConstants.Wardrobes.HAIR_CATEGORY_ID;
        }
    }
}