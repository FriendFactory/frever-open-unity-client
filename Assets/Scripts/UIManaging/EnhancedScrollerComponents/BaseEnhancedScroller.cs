using System;
using System.Collections.Generic;

namespace UIManaging.EnhancedScrollerComponents
{
    public class BaseEnhancedScroller<T>
    {
        public event Action OnItemsChangedEvent;
        
        public IList<T> Items { get; private set; }

        public BaseEnhancedScroller(IList<T> items)
        {
            Items = items;
        }

        public void SetItems(IList<T> items)
        {
            Items = items;
            OnItemsChangedEvent?.Invoke();
        }
    }
}