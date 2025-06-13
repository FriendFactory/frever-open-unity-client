using System.Collections.Generic;

namespace UIManaging.Pages.Common.SongOption.Playlists
{
    public class PlaylistsViewModel : MusicViewModel
    {
        public string Name { get; }
        public IEnumerable<PlaylistItemModel> Playlists { get; }

        public PlaylistsViewModel(string name, IEnumerable<PlaylistItemModel> playlistModels)
        {
            Name = name;
            Playlists = playlistModels;
        }
    }
}