using UIManaging.Pages.Common.SongOption.SongList;

namespace UIManaging.Pages.Common.SongOption.Search
{
    internal class MoodsSearchView : BasePlayableSearchView<PlayableSongModel, SongSearchListModel, SongSearchList>
    {
        public override MusicSearchType MusicSearchType => MusicSearchType.Moods;
    }
}