using System.Collections.Generic;

namespace UIManaging.Pages.Common.SongOption.SongDiscovery
{
    public class GenresViewModel : MusicViewModel
    {
        public IEnumerable<GenreButtonModel> Playlists { get; }
        
        public GenresViewModel(IEnumerable<GenreButtonModel> playlists)
        {
            Playlists = playlists;
        }
    }
}