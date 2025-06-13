using Bridge.Models.Common;
using UIManaging.Pages.Common.SongOption.Common;

namespace UIManaging.Pages.Common.SongOption.SongList
{
    public class PlayableTrendingUserSoundModel: PlayableItemModel
    {
        public PlayableTrendingUserSound TrendingUserSound { get; }

        public override long Id => TrendingUserSound.UserSound.Id;
        public override string Title => TrendingUserSound.UserSound.Name;
        public override string ArtistName => TrendingUserSound.Owner?.Nickname ?? "Trending";
        public override IPlayableMusic Music => TrendingUserSound;

        public PlayableTrendingUserSoundModel(PlayableTrendingUserSound trendingUserSound)
        {
            TrendingUserSound = trendingUserSound;
        }
    }
}