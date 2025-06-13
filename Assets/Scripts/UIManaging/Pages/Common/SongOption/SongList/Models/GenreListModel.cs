using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Common;
using UIManaging.Pages.Common.SongOption.SongDiscovery;
using Genre = Bridge.Models.ClientServer.StartPack.Metadata.Genre;

namespace UIManaging.Pages.Common.SongOption.SongList
{
    internal class GenreListModel
    {
        private readonly MusicDataProvider _musicDataProvider;
        private readonly MusicSelectionStateController _musicSelectionStateController;
        private readonly List<GenrePanelModel> _genrePanelData;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public IReadOnlyList<GenrePanelModel> GenrePanelsData => _genrePanelData;

        public DiscoveryPanelModel DiscoveryPanelModel { get; }
        public bool CommercialOnly { get; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public GenreListModel(MusicDataProvider musicDataProvider, MusicSelectionStateController musicSelectionStateController, bool commercialOnly)
        {
            _musicDataProvider = musicDataProvider;
            _musicSelectionStateController = musicSelectionStateController;
            _genrePanelData = _musicDataProvider.Genres
                                .Select(gender => new GenrePanelModel(gender, commercialOnly, DisplayGenreList))
                                .OrderBy(model => model.Genre.SortOrder)
                                .ToList();
            
            //todo: remove when we have implemented long term solution FREV-8056
            var genres = AddNewestSongsGenreIfNeeded(_musicDataProvider.Genres);

            CommercialOnly = commercialOnly;
            DiscoveryPanelModel = new DiscoveryPanelModel(genres, DisplayGenreList);
        }

        private static IEnumerable<Genre> AddNewestSongsGenreIfNeeded(ICollection<Genre> allGenres)
        {
            //prevent having 2 "New" categories, if app starts to get that from backend side
            if (allGenres.Any(x => x.Name == Constants.Genres.NEW_GENRE_NAME)) return allGenres;
            
            return allGenres.Prepend(new Genre
            {
                Id = Constants.Genres.NEW_GENRE_ID,
                Name = Constants.Genres.NEW_GENRE_NAME,
                SortOrder = Constants.Genres.NEW_GENRE_SORT_ORDER
            }).OrderBy(genre => genre.SortOrder).ToArray();
        }
        
        private void DisplayGenreList(long genreId)
        {
            var playlistModel = new FullPlaylistListModel()
            {
                Name = GetGenreName(genreId),
                Playables = GetSongsForGenre(genreId).ToArray()
            };
            
            _musicSelectionStateController.FireAsync(MusicNavigationCommand.OpenPlaylist, playlistModel);
        }
        
        private IEnumerable<SongInfo> GetSongsForGenre(long genreId)
        {
            var songs = CommercialOnly ? _musicDataProvider.CommercialSongs : _musicDataProvider.Songs;

            return genreId == Constants.Genres.NEW_GENRE_ID
                ? songs.OrderByDescending(x => x.Id).Take(50)
                : songs.Where(song => song.GenreId == genreId);
        }
        
        private string GetGenreName(long genreId)
        {
            return genreId == Constants.Genres.NEW_GENRE_ID 
                ? Constants.Genres.NEW_GENRE_NAME 
                : _musicDataProvider.Genres.First(x=>x.Id == genreId).Name;
        }
    }
}