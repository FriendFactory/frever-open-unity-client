using Bridge.Models.ClientServer.Assets;

namespace Extensions.Wardrobe
{
    public static class OutfitExtensions
    {
        public static OutfitShortInfo ToShortInfo(this OutfitFullInfo outfitFullInfo)
        {
            return new OutfitShortInfo()
            {
                Id = outfitFullInfo.Id,
                Name = outfitFullInfo.Name,
                SaveMethod = outfitFullInfo.SaveMethod,
                Files = outfitFullInfo.Files,
            };
        }
    }
}