using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Results;
using JetBrains.Annotations;
using Navigation.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Modules.MusicCacheManaging
{
    [UsedImplicitly]
    internal sealed class LicensedMusicProvider: ILicensedMusicProvider
    {
        private static readonly PageId[] CLEAR_CACHE_ON_PAGES = { PageId.Feed, PageId.UserProfile, PageId.CreatePost, PageId.HomePage, PageId.DraftsPage };
        
        private readonly IExternalMusicBridge _bridge;
        private readonly PageManager _pageManager;
        private readonly Dictionary<long, AudioClip> _cachedSongs = new Dictionary<long, AudioClip>();
        private readonly List<long> _songsIdToPutInCacheWhenLoaded = new List<long>();

        private readonly Dictionary<long, Task<Result<AudioClip>>> _currentlyLoading =
            new Dictionary<long, Task<Result<AudioClip>>>();

        private bool _initialized;

        public IEnumerable<long> KeptInMemoryClipIds => _cachedSongs.Keys;

        public LicensedMusicProvider(IExternalMusicBridge bridge, PageManager pageManager)
        {
            _bridge = bridge;
            _pageManager = pageManager;
        }

        public void Initialize()
        {
            if (_initialized) return;
            _pageManager.PageDisplayed += OnPageLoaded;
            _initialized = true;
        }

        public async Task<AudioClipResponse> GetExternalTrackClip(long trackId, CancellationToken cancellationToken = default)
        {
            if (_cachedSongs.TryGetValue(trackId, out var cachedSong))
            {
                return new AudioClipResponse
                {
                    AudioClip = cachedSong
                };
            }

            Result<AudioClip> resp;
            if (_currentlyLoading.ContainsKey(trackId))
            {
                resp = await _currentlyLoading[trackId];
            }
            else
            {
                var loadingTask = _bridge.DownloadExternalTrackClip(trackId, cancellationToken);
                _currentlyLoading.Add(trackId, loadingTask);
                resp = await loadingTask;
                _currentlyLoading.Remove(trackId);
            }
            
            if (resp.IsSuccess && _songsIdToPutInCacheWhenLoaded.Contains(trackId))
            {
                KeepClipInMemoryCache(trackId, resp.Model);
            }
            
            return new AudioClipResponse
            {
                AudioClip = resp.Model,
                ErrorMessage = resp.ErrorMessage,
                RequestCancelled = resp.IsRequestCanceled
            };
        }

        public void KeepClipInMemoryCache(long trackId, AudioClip clip)
        {
            if (clip == null) throw new ArgumentNullException(nameof(clip));
            
            _cachedSongs[trackId] = clip;
            if (_songsIdToPutInCacheWhenLoaded.Contains(trackId))
            {
                _songsIdToPutInCacheWhenLoaded.Remove(trackId);
            }
        }

        public void KeepInCacheWhenLoaded(long trackId)
        {
            if (!_songsIdToPutInCacheWhenLoaded.Contains(trackId) && !_cachedSongs.ContainsKey(trackId))
            {
                _songsIdToPutInCacheWhenLoaded.Add(trackId);
            }
        }
        
        public void DontKeepAfterLoading(long trackId)
        {
            if (_songsIdToPutInCacheWhenLoaded.Contains(trackId))
            {
                _songsIdToPutInCacheWhenLoaded.Remove(trackId);
            }
        }

        public void ClearAutoKeepInCacheRegister()
        {
            _songsIdToPutInCacheWhenLoaded.Clear();
        }

        public bool IsKeptByCache(long trackId)
        {
            return _cachedSongs.ContainsKey(trackId);
        }

        public void RemoveFromInMemoryCache(long trackId)
        {
            if (!_cachedSongs.TryGetValue(trackId, out var clip)) return;
            Object.Destroy(clip);
            _cachedSongs.Remove(trackId);
        }

        public void ClearInMemoryCache()
        {
            foreach (var cachedSong in _cachedSongs)
            {
                Object.Destroy(cachedSong.Value);
            }
            
            _cachedSongs.Clear();
            
            _songsIdToPutInCacheWhenLoaded.Clear();
        }
        
        private void OnPageLoaded(PageData pageData)
        {
            if (CLEAR_CACHE_ON_PAGES.Contains(pageData.PageId))
            {
                ClearInMemoryCache();
            }
        }
    }
}