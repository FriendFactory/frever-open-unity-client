using System;
using System.Collections.Generic;
using System.Linq;
using Genre = Bridge.Models.ClientServer.StartPack.Metadata.Genre;

namespace UIManaging.Pages.Common.SongOption.SongDiscovery
{
    public class DiscoveryPanelModel
    {
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public IReadOnlyList<GenreButtonModel> AllGenres { get; }
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public DiscoveryPanelModel(IEnumerable<Genre> allGenres, Action<long> onSelect)
        {
            AllGenres = allGenres.Select(genre => new GenreButtonModel(genre.Name, genre.Id, onSelect)).ToList();
        }
    }
}