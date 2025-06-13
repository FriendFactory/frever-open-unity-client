using Bridge.Services._7Digital.Models.PlaylistModels;

namespace UIManaging.Pages.Common.SongOption.Playlists
{
    public class PlaylistItemModel
    {
        public PlaylistInfo PlaylistInfo { get; }
        
        public PlaylistItemModel(PlaylistInfo playlistInfo)
        {
            PlaylistInfo = playlistInfo;
        }
    }
}