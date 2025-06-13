using UIManaging.Pages.Common.SongOption.SongList;

namespace UIManaging.Pages.Common.SongOption.Search
{
    internal class TrackSearchView : BasePlayableSearchView<PlayableTrackModel, TrackSearchListModel, TrackSearchList>
    {
        public override MusicSearchType MusicSearchType => MusicSearchType.Music;
    }
}