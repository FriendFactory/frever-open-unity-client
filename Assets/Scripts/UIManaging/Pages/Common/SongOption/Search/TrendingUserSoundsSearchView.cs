using UIManaging.Pages.Common.SongOption.SongList;

namespace UIManaging.Pages.Common.SongOption.Search
{
    internal class TrendingUserSoundsSearchView : BasePlayableSearchView<PlayableTrendingUserSoundModel,
        TrendingUserSoundsSearchListModel, TrendingUserSoundSearchList>
    {
        public override MusicSearchType MusicSearchType => MusicSearchType.TrendingUserSounds;
    }
}