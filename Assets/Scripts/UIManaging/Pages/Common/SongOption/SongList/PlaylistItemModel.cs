namespace UIManaging.Pages.Common.SongOption.SongList
{
    public sealed class PlaylistItemModel
    {
        public string Title { get;}
        public PlayableTrackModel[] Tracks { get;}
        
        public PlaylistItemModel(string title, PlayableTrackModel[] tracks)
        {
            Title = title;
            Tracks = tracks;
        }
    }
}
