using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.Common;
using Bridge.Services._7Digital.Models.PlaylistModels;
using Bridge.Services.Advertising;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using UIManaging.Pages.Common.FavoriteSounds;
using UIManaging.Pages.Common.SongOption.SongList;
using Zenject;

namespace UIManaging.Pages.Common.SongOption
{
    [UsedImplicitly]
    public class MusicDataProvider: IInitializable, IDisposable
    {
        private const int PROMOTED_SONGS_TAKE = 10;
        private const int TRENDING_USER_SOUNDS_TAKE = 500;
        
        private readonly IBridge _bridge;
        private readonly IDataFetcher _dataFetcher;
        private readonly SoundsFavoriteStatusCache _favoriteStatusCache;
        private readonly CancellationTokenSource _tokenSource;
        
        private PromotedSong[] _promotedSongs;
        private SongInfo[] _commercialSongs;
        
        //---------------------------------------------------------------------
        // Properties 
        //---------------------------------------------------------------------
        
        public bool IsInitialized { get; private set; }
        public List<Genre> Genres => _dataFetcher.MetadataStartPack.Genres;
        public IEnumerable<SongInfo> Songs => _dataFetcher.AllSongs;
        public IEnumerable<SongInfo> CommercialSongs => _commercialSongs;
        public IEnumerable<PlaylistInfo> Playlists => _dataFetcher.AllPlaylists;
        public IEnumerable<PromotedSong> PromotedSongs => _promotedSongs;
        
        //---------------------------------------------------------------------
        // Ctors 
        //---------------------------------------------------------------------

        public MusicDataProvider(IBridge bridge, IDataFetcher dataFetcher, SoundsFavoriteStatusCache favoriteStatusCache)
        {
            _bridge = bridge;
            _dataFetcher = dataFetcher;
            _favoriteStatusCache = favoriteStatusCache;
            _tokenSource = new CancellationTokenSource();
        }
        
        //---------------------------------------------------------------------
        //  Public
        //---------------------------------------------------------------------

        public async void Initialize()
        {
            try
            {
                await InitializeAsync(_tokenSource.Token);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public IEnumerable<PlaylistItemModel> GetPlaylistsModels()
        {
            return _dataFetcher.AllPlaylists?
                        .Where(x => x != null && !x.Tracks.IsNullOrEmpty())
                        .Select(x =>
                         {
                             var trackModels = x.Tracks
                                                .Select(track => new PlayableTrackModel(track, _favoriteStatusCache.IsFavorite(track)))
                                                .ToArray();
                             return new PlaylistItemModel(x.Title, trackModels);
                         });
        }

        public void Dispose()
        {
            _tokenSource?.CancelAndDispose();
        }
        
        //---------------------------------------------------------------------
        //  Helpers
        //---------------------------------------------------------------------

        private async Task InitializeAsync(CancellationToken token)
        {
            var tasks = new []
            {
                _favoriteStatusCache.InitializeAsync(token),
                FetchPromotedSongsAsync(token),
                FetchCommercialSongsAsync(token),
            };
            
            await Task.WhenAll(tasks);
            
            IsInitialized = true;
        }
        
        private async Task FetchPromotedSongsAsync(CancellationToken token)
        {
            var result = await _bridge.GetPromotedSongs(PROMOTED_SONGS_TAKE, 0, token);  
            if (result.IsError)  
            {  
                Debug.LogError("No promoted songs available. [Reason]: " + result.ErrorMessage);  
                return;
            }
            
            if (result.IsRequestCanceled) return;

            _promotedSongs = result.Models;
        }

        private async Task FetchCommercialSongsAsync(CancellationToken token)
        {
            var result = await _bridge.GetSongsAsync(TRENDING_USER_SOUNDS_TAKE, 0, commercialOnly: true, token: token);
            if (result.IsError)
            {
                Debug.LogError($"[{GetType().Name}] Failed to get trending user sounds # {result.ErrorMessage}");
                return;
            }
            
            if (result.IsRequestCanceled) return;

            _commercialSongs = result.Models;
            
            AddToFavoritesCache(_commercialSongs);
        }

        private void AddToFavoritesCache(IEnumerable<IPlayableMusic> sounds)
        {
            sounds.ForEach(sound => _favoriteStatusCache.AddToCacheIfNeeded(sound));
        }
    }
}