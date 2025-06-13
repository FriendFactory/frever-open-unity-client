using Bridge.Models.ClientServer.Assets;
using System.Linq;

namespace Modules.WardrobeManaging
{
    public class PresetItem : WardrobeItem
    {
        public UmaBundleFullInfo PresetBundle { get; }

        public string AssetName { get; }

        public string PresetCategory { get; }

        public PresetItem(WardrobeFullInfo wardrobe, UmaBundleFullInfo bundle, string presetCategory) : base(wardrobe)
        {
            PresetBundle = bundle;
            AssetName = bundle.UmaAssets.First().UmaAssetFiles.First().Name;
            PresetCategory = presetCategory;
        }
    }
}