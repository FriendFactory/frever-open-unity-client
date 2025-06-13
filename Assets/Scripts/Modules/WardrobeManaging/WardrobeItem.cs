using Bridge.Models.ClientServer.Assets;

namespace Modules.WardrobeManaging
{
    public class WardrobeItem
    {
        public WardrobeFullInfo Wardrobe { get; }

        public bool IsEmpty { get; }

        public WardrobeItem(WardrobeFullInfo wardrobe)
        {
            Wardrobe = wardrobe;
            IsEmpty = wardrobe is null;
        }
    }
}