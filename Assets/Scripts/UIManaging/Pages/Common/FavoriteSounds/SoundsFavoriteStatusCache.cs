using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abstract;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Bridge.Services._7Digital.Models.TrackModels;
using Extensions;
using JetBrains.Annotations;
using Modules.MusicLicenseManaging;
using UIManaging.Pages.Common.SongOption.Common;
using UnityEngine;

namespace UIManaging.Pages.Common.FavoriteSounds
{
    /// <summary>
    /// Represents a local cache to keep track of sound favorite status.
    /// Each sound is added to cache only once and then all favorite status updates go through cache, allowing to not update model itself until next user request
    /// </summary>
    [UsedImplicitly]
    public sealed class SoundsFavoriteStatusCache: IAsyncInitializable
    {
        private const int TAKE = 200;
        
        private readonly IBridge _bridge;
        private readonly MusicLicenseValidator _musicLicenseValidator;

        private readonly Dictionary<SoundType, Dictionary<long, bool>> _cacheMap = new Dictionary<SoundType, Dictionary<long, bool>>()
        {
            { SoundType.ExternalSong, new Dictionary<long, bool>() },
            { SoundType.Song, new Dictionary<long, bool>() },
            { SoundType.UserSound, new Dictionary<long, bool>() },
        };
        
        public bool IsInitialized { get; set; }

        public event Action<SoundFavoriteStatusChangedEventArgs> SoundFavoriteStatusChanged;

        public SoundsFavoriteStatusCache(IBridge bridge, MusicLicenseValidator musicLicenseValidator)
        {
            _bridge = bridge;
            _musicLicenseValidator = musicLicenseValidator;
        }
        
        public async Task InitializeAsync(CancellationToken token = default)
        {
            await LoadAllModelsAsync(token);
            
            if (token.IsCancellationRequested) return;
            
            IsInitialized = true;
        }

        public void CleanUp() { }
        
        public bool IsFavorite(IPlayableMusic sound)
        {
            if (sound == null) return false;
            
            sound = sound is PlayableTrendingUserSound trendingUserSound ? trendingUserSound.UserSound : sound; 
            var type = sound.GetFavoriteSoundType();
            var id = sound.Id;

            return TryGetCache(type, out var items) && items.TryGetValue(id, out var isFavorite) && isFavorite;
        }
        
        /// <summary>
        /// Add sound to cache only if sound is favorite.
        /// </summary>
        public void AddToCacheIfNeeded(IPlayableMusic sound)
        {
            if (sound == null) return;
            
            if (IsInCache(sound) || !IsSoundFavorite(sound)) return;
            
            sound = sound is PlayableTrendingUserSound trendingUserSound ? trendingUserSound.UserSound : sound; 
            var soundType = sound.GetFavoriteSoundType();
            var isFavorite = IsSoundFavorite(sound);
            var id = sound.Id;
            
            AddOrUpdate(soundType, id, isFavorite);
        }

        public void AddOrUpdate(SoundType type, long id, bool isFavorite)
        {
            if (!TryGetCache(type, out var items)) return;

            items[id] = isFavorite;
            
            SoundFavoriteStatusChanged?.Invoke(new SoundFavoriteStatusChangedEventArgs(type, id, isFavorite));
        }
        
        private async Task LoadAllModelsAsync(CancellationToken token)
        {
            var skip = 0;

            while (true)
            {
                var result = await _bridge.GetFavouriteSoundList(TAKE, skip, false, token);
                if (result.IsError)
                {
                    Debug.LogError($"[{GetType().Name}] Failed to get models # {result.ErrorMessage}");
                    break;
                }
                
                if (token.IsCancellationRequested) return;

                // TODO: use only for ExternalSong type when GetTrendingUserSounds receives support for IsFavorite
                result.Models.Where(model => model.Type != SoundType.Song)
                      .ForEach(model =>
                       {
                           if (model.Type == SoundType.ExternalSong && !_musicLicenseValidator.IsPremiumSoundsEnabled) return;
                           
                           if (!_cacheMap.TryGetValue(model.Type, out var cache)) return;
                           
                           cache[model.Id] = true;
                       });
                
                if (result.Models.Length < TAKE) break;

                skip += TAKE;
            }
        }

        private bool TryGetCache(SoundType type, out Dictionary<long, bool> cache)
        {
            switch (type)
            {
                case SoundType.Song:
                    cache = _cacheMap[SoundType.Song];
                    break;
                case SoundType.ExternalSong:
                    cache = _cacheMap[SoundType.ExternalSong];
                    break;
                case SoundType.UserSound:
                    cache = _cacheMap[SoundType.UserSound];
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return cache != null;
        }

        private bool IsInCache(IPlayableMusic sound)
        {
            sound = sound is PlayableTrendingUserSound trendingUserSound ? trendingUserSound.UserSound : sound;
            var type = sound.GetFavoriteSoundType();
            var id = sound.Id;

            return TryGetCache(type, out var items) && items.ContainsKey(id);
        }

        private bool IsSoundFavorite(IPlayableMusic sound)
        {
            switch (sound)
            {
                case FavouriteMusicInfo _:
                    return true;
                case SongInfo song:
                    return song.IsFavorite;
                case UserSoundFullInfo userSound:
                    return userSound.IsFavorite;
                case PlayableTrendingUserSound trendingUserSound:
                    return trendingUserSound.UserSound.IsFavorite;
                case ExternalTrackInfo externalTrack:
                    var cache = _cacheMap[SoundType.ExternalSong];
                    return cache.TryGetValue(externalTrack.Id, out var isFavorite) && isFavorite;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sound));
            }
        }
    }

    public class SoundFavoriteStatusChangedEventArgs
    {
        public SoundType Type { get; }
        public long Id { get; }
        public bool IsFavorite { get; }

        public SoundFavoriteStatusChangedEventArgs(SoundType type, long id, bool isFavorite)
        {
            Type = type;
            Id = id;
            IsFavorite = isFavorite;
        }
    }
}