using System.Collections.Generic;
using System.Linq;
using Bridge.Services._7Digital.Models.PlaylistModels;

namespace UIManaging.Pages.Common.SongOption.Playlists
{
    public class PlaylistItemsModel
    {
        public IEnumerable<PlaylistItemModel> Playlists { get; }
        
        public PlaylistItemsModel(IEnumerable<PlaylistInfo> playlists)
        {
            Playlists = playlists.Select(playlist => new PlaylistItemModel(playlist));
        }
    }
}