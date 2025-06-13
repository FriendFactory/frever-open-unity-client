using System;
using System.Collections.Generic;
using Bridge.Models.Common;
using Genre = Bridge.Models.ClientServer.StartPack.Metadata.Genre;

namespace UIManaging.Pages.Common.SongOption.SongList
{
    public class GenrePanelModel
    {
        //---------------------------------------------------------------------
        // Fields
        //---------------------------------------------------------------------
        
        private readonly Action<long> _onShowFullList;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public Genre Genre { get; }
        public bool CommercialOnly { get; }
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public GenrePanelModel(Genre genre, bool commercialOnly, Action<long> onShowFullList)
        {
            Genre = genre;
            CommercialOnly = commercialOnly;
            _onShowFullList = onShowFullList;
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void ShowFullList() => _onShowFullList?.Invoke(Genre.Id);
    }
}