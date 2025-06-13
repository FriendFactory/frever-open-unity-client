using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Modules.MusicCacheManaging
{
    
    /// <summary>
    /// Provides API for loading licensing songs from 3rd providers
    /// Keeps music AudioClips in InMemory cache, because we can't store them locally
    /// </summary>
    public interface ILicensedMusicProvider
    {
        IEnumerable<long> KeptInMemoryClipIds { get; }
        
        void Initialize();
        Task<AudioClipResponse> GetExternalTrackClip(long trackId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Keeps in cache already loaded clip
        /// </summary>
        void KeepClipInMemoryCache(long trackId, AudioClip clip);
       
        /// <summary>
        /// Puts audio clip into cache on song loading
        /// </summary>
        void KeepInCacheWhenLoaded(long trackId);
       
        /// <summary>
        /// Prevents previously requested audio clip auto putting into cache when the song is loaded
        /// </summary>
        void DontKeepAfterLoading(long trackId);
        void ClearAutoKeepInCacheRegister();
        
        /// <summary>
        /// Checks if the audio clip is loaded and kept by cache
        /// </summary>
        bool IsKeptByCache(long trackId);
        void RemoveFromInMemoryCache(long trackId);
        void ClearInMemoryCache();
    }

    public struct AudioClipResponse
    {
        public AudioClip AudioClip;
        public string ErrorMessage;
        public bool RequestCancelled;
    }
}
