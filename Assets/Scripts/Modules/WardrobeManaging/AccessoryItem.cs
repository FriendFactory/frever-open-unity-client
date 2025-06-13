using Bridge.Models.ClientServer.Assets;

namespace Modules.WardrobeManaging
{
    public class AccessoryItem : WardrobeItem
    {
        public string Slot { get; }

        public string AssetName { get; }

        public AccessoryItem(WardrobeFullInfo wardrobe, string slot, string assetName) : base(wardrobe)
        {
            Slot = slot;
            AssetName = assetName;
        }
    }
}