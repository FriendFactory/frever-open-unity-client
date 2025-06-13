using System;

namespace Common.Pools
{
    public interface IPool<T>
    {
        T Get(Func<T, bool> predicate = null);
        
        /// <summary>
        /// Manual way to add a new item
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <param name="ready">Ready to use</param>
        void Add(T item, bool ready);

        /// <summary>
        /// All items become ready to use
        /// </summary>
        void Reset();
        
        /// <summary>
        /// Removal (Destroying) of all items
        /// </summary>
        void Clear();
    }
}