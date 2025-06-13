using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Services._7Digital.Models.TrackModels;
using UIManaging.Pages.Common.SongOption.SongList;

namespace UIManaging.Pages.Common.SongOption.MusicCategory
{
    public class PlaylistViewModel
    {
        private readonly List<PlaylistPanelModel> _playlistPanelModels;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public IReadOnlyList<PlaylistPanelModel> PlaylistPanelModels => _playlistPanelModels;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public PlaylistViewModel(PlaylistItemModel[] playlistInfos, Action<string, ExternalTrackInfo[]> onShowFullList)
        {
            _playlistPanelModels = playlistInfos.Select(x => new PlaylistPanelModel(x.Title, x.Tracks, onShowFullList)).ToList();
        }
    }
}
