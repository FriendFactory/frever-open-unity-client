using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Modules.AssetsManaging.Loaders
{
    public interface ISetLocationBackgroundInMemoryCacheControl
    {
        void SetCapacity(int capacity);
        void Clear(params long[] except);
    }

    internal interface ISetLocationBackgroundInMemoryCache: ISetLocationBackgroundInMemoryCacheControl
    {
        void Add(long id, Texture2D texture2D);
        void Remove(long id);
        bool TryGetBackgroundTexture(long id, out Texture2D texture2D);
    }

    [UsedImplicitly]
    internal sealed class SetLocationBackgroundInMemoryCache: ISetLocationBackgroundInMemoryCache
    {
        private int _capacity = 5;

        private readonly List<SetLocationBackgroundData> _cache = new List<SetLocationBackgroundData>();

        public void SetCapacity(int capacity)
        {
            _capacity = Mathf.Max(capacity, 0);
            ClearToFitCapacity();
        }

        public void Add(long id, Texture2D texture2D)
        {
            if (_cache.Any(x => x.Id == id))
            {
                return;
            }
            
            _cache.Add(new SetLocationBackgroundData
            {
                Id = id,
                Texture2D = texture2D,
                LastUsedTime = DateTime.Now
            });
            ClearToFitCapacity();
        }

        public void Remove(long id)
        {
            var data = _cache.FirstOrDefault(x => x.Id == id);
            if (data == null) return;
            Object.Destroy(data.Texture2D);
            _cache.Remove(data);
        }

        public bool TryGetBackgroundTexture(long id, out Texture2D texture2D)
        {
            var data = _cache.FirstOrDefault(x => x.Id == id);
            if (data == null)
            {
                texture2D = null;
                return false;
            }

            texture2D = data.Texture2D;
            data.LastUsedTime = DateTime.Now;
            return true;
        }

        public void Clear(params long[] except)
        {
            for (var i = _cache.Count - 1; i >= 0; i--)
            {
                var data = _cache[i];
                if (!except.IsNullOrEmpty() && except.Contains(data.Id)) continue;
                Remove(data.Id);
            }
        }

        private void ClearToFitCapacity()
        {
            if (_cache.Count <= _capacity) return;
            var needToUnloadCount = _cache.Count - _capacity;
            var backgroundsToUnload = _cache.OrderBy(x => x.LastUsedTime).Take(needToUnloadCount).ToArray();
            foreach (var data in backgroundsToUnload)
            {
                Remove(data.Id);
            }
        }

        private sealed class SetLocationBackgroundData
        {
            public long Id;
            public DateTime LastUsedTime;
            public Texture2D Texture2D;
        }
    }
}