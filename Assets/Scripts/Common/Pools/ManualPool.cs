using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Pools
{
    /// <summary>
    /// Implementation of <see cref="IPool{T}"/> with manual control on items.
    /// New items should be added manually, otherwise pool will be always empty.
    /// </summary>
    public sealed class ManualPool<T> : IPool<T>
        where T : class, IPoolable<T>
    {
        private readonly IList<T> _ready = new List<T>();
        private readonly IList<T> _busy = new List<T>();

        readonly Action<T> _destroy;

        public ManualPool(Action<T> destroy)
        {
            _destroy = destroy ?? throw new ArgumentNullException(nameof(destroy));
        }

        //---------------------------------------------------------------------
        // IPool
        //---------------------------------------------------------------------
        T IPool<T>.Get(Func<T, bool> predicate)
        {
            T item = null;
            if (0 < _ready.Count)
            {
                item = predicate != null
                    ? _ready.FirstOrDefault(predicate)
                    : _ready[0];

                if (item != null)
                {
                    _ready.Remove(item);
                    AddToBusy(item);
                }
            }

            return item;
        }

        void IPool<T>.Add(T item, bool ready)
        {
            SubscribeToItem(item);
            if (ready)
            {
                AddToReady(item);
            }
            else
            {
                AddToBusy(item);
            }
        }

        void IPool<T>.Reset()
        {
            foreach (var item in _busy)
            {
                AddToReady(item);
            }
        }

        void IPool<T>.Clear()
        {
            foreach (var item in _busy) DestroyItem(item);
            _busy.Clear();

            foreach (var item in _ready) DestroyItem(item);
            _ready.Clear();
        }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        private void SubscribeToItem(T item)
        {
            item.Used += OnItemUsed;
        }

        private void UnsubscribeFromItem(T item)
        {
            item.Used -= OnItemUsed;
        }

        private void OnItemUsed(T item)
        {
            if (_busy.Remove(item))
            {
                AddToReady(item);
            }
        }

        //---------------------------------------------------------------------
        // Other
        //---------------------------------------------------------------------

        private void AddToBusy(T item)
        {
            _busy.Add(item);
            item.Visible = true;
        }

        private void AddToReady(T item)
        {
            item.Visible = false;
            _ready.Add(item);
        }

        private void DestroyItem(T existingItem)
        {
            UnsubscribeFromItem(existingItem);
            _destroy(existingItem);
        }
    }
}