using System.Collections.Generic;
using System.Linq;

namespace Common.Collections
{
    /// <summary>
    /// A dictionary-based cache that could limit the number of items it can hold.
    /// </summary>
    /// <typeparam name="TValue">The type of items to be stored in the cache.</typeparam>
    public class DictionaryCache<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _cache = new Dictionary<TKey, TValue>();
        private readonly object _lockObject = new object();
        private readonly int _maxSize;

        /// /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryCache{TKey,TValue}"/> class with the specified maximum size.
        /// </summary>
        /// <param name="maxSize">The maximum number of items the cache can hold. The default value is 5</param>
        public DictionaryCache(int maxSize = 5)
        {
            _maxSize = maxSize;
        }

        public void Add(TKey key, TValue value)
        {
            lock (_lockObject)
            {
                if (_cache.ContainsKey(key)) return;
                
                // If the cache has already reached its maximum size,
                // remove the oldest item before adding a new one
                if (_cache.Count >= _maxSize)
                {
                    var oldestKey = _cache.Keys.First();
                    if (oldestKey != null) _cache.Remove(oldestKey);
                }

                _cache.Add(key, value);
            }
        }

        public bool TryGet(TKey key, out TValue value)
        {
            lock (_lockObject)
            {
                return _cache.TryGetValue(key, out value);
            }
        }

        public bool Remove(TKey key)
        {
            lock (_lockObject)
            {
                return _cache.Remove(key);
            }
        }

        public void Clear()
        {
            lock (_lockObject)
            {
                _cache.Clear();
                
            }
        }
    }
}