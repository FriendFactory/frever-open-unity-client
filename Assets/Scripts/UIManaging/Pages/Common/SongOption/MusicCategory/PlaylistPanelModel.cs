using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Services._7Digital.Models.TrackModels;
using UIManaging.Pages.Common.SongOption.SongList;

namespace UIManaging.Pages.Common.SongOption.MusicCategory
{
    public sealed class PlaylistPanelModel
    {
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        private readonly Action<string, ExternalTrackInfo[]> _onShowFullList;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
    
        public IReadOnlyList<PlayableTrackModel> Tracks { get; }
        public string PlaylistName { get; }
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public PlaylistPanelModel(string playlistName, PlayableTrackModel[] trackInfos, Action<string, ExternalTrackInfo[]> onShowFullList)
        {
            PlaylistName = playlistName;
            Tracks = trackInfos;
            _onShowFullList = onShowFullList;
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void ShowFullList() => _onShowFullList?.Invoke(PlaylistName, Tracks.Select(x=>x.TrackInfo).ToArray());
    }
}
