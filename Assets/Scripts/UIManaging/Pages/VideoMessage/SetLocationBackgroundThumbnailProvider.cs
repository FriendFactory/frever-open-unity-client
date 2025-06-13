using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.VideoMessage
{
    [UsedImplicitly]
    internal sealed class SetLocationBackgroundThumbnailProvider
    {
        private const int CACHE_SIZE_LIMIT = 30;

        private readonly IBridge _bridge;
        private readonly List<ThumbnailData> _cache = new List<ThumbnailData>();
        private readonly List<ThumbnailKey> _loadingItems = new List<ThumbnailKey>();

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public SetLocationBackgroundThumbnailProvider(IBridge bridge)
        {
            _bridge = bridge;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async Task<Texture2D> GetThumbnailAsync(IBackgroundOption model, CancellationToken token = default)
        {
            var key = new ThumbnailKey { BackgroundId = model.Id, Type = model.Type };

            while (_loadingItems.Contains(key) && !token.IsCancellationRequested)
            {
                await Task.Delay(33, token);
            }
            
            var cached = _cache.FirstOrDefault(x => x.Key.Equals(key));
            if (cached != null)
            {
                cached.LastTimeUsed = DateTime.UtcNow;
                return cached.Texture2D;
            }
            
            _loadingItems.Add(key);
            var textureResult = await _bridge.GetThumbnailAsync(model, Resolution._128x128, cancellationToken: token);
            _loadingItems.Remove(key);

            if (textureResult.IsSuccess)
            {
                _cache.Add(new ThumbnailData
                {
                    Key = key,
                    LastTimeUsed = DateTime.UtcNow,
                    Texture2D = textureResult.Object as Texture2D
                } );
            }
            
            if (_cache.Count > CACHE_SIZE_LIMIT)
            {
                CleanupToFitCacheSize();
            }
            
            return textureResult.Object as Texture2D;
        }

        public void Cleanup()
        {
            foreach (var thumbnailData in _cache)
            {
                thumbnailData.Release();
            }
            _cache.Clear();
        }

        private void CleanupToFitCacheSize()
        {
            var itemsToDrop = _cache.OrderBy(x => x.LastTimeUsed).Take(CACHE_SIZE_LIMIT - _cache.Count).ToArray();
            foreach (var item in itemsToDrop)
            {
                item.Release();
                _cache.Remove(item);
            }
        }
        
        private sealed class ThumbnailData
        {
            public ThumbnailKey Key;
            public Texture2D Texture2D;
            public DateTime LastTimeUsed;

            public void Release()
            {
                Object.Destroy(Texture2D);
            }
        }

        private struct ThumbnailKey
        {
            public long BackgroundId;
            public BackgroundOptionType Type;
        }
    }
}