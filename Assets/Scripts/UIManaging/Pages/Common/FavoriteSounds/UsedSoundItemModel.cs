using Bridge.Models.Common;

namespace UIManaging.Pages.Common.FavoriteSounds
{
    public class UsedSoundItemModel: SoundItemModel
    {
        public int UsageCount { get; }

        public UsedSoundItemModel(IPlayableMusic sound, bool isFavorite, int usageCount) : base(sound, isFavorite)
        {
            UsageCount = usageCount;
        }
    }
}