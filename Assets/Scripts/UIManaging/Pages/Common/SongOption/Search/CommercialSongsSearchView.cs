using UIManaging.Pages.Common.SongOption.SongList;

namespace UIManaging.Pages.Common.SongOption.Search
{
    internal class CommercialSongsSearchView: BasePlayableSearchView<PlayableSongModel, CommercialSongSearchListModel, CommercialSongSearchList>
    {
        public override MusicSearchType MusicSearchType => MusicSearchType.CommercialSongs;
    }
}