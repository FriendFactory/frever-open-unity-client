using System;
using UnityEngine;

namespace UIManaging.Pages.Common.SongOption.SongDiscovery
{
    public class GenreButtonModel
    {
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        private readonly Action<long> _onSelect;
        
        //---------------------------------------------------------------------
        // Fields
        //---------------------------------------------------------------------
        
        private long _genreId;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public string Name { get; }

        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public GenreButtonModel(string name, long genreId, Action<long> onSelect)
        {
            Name = name;
            _genreId = genreId;
            _onSelect = onSelect;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void OnSelect() => _onSelect?.Invoke(_genreId);
    }
}