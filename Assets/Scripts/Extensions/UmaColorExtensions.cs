using System.Collections.Generic;
using Bridge.Models.ClientServer.Assets;

namespace Extensions
{
    public static class UmaColorExtensions
    {
        public static List<UmaSharedColorInfo> ConvertToOutfitAndUmaSharedColor(this IEnumerable<KeyValuePair<long, int>> sharedColors)
        {
            var outfitSharedColors = new List<UmaSharedColorInfo>();

            foreach (var item in sharedColors)
            {
                var outfitColor = new UmaSharedColorInfo()
                {
                    UmaSharedColorId = item.Key,
                    Color = item.Value
                };

                outfitSharedColors.Add(outfitColor);
            }

            return outfitSharedColors;
        }
    }
}