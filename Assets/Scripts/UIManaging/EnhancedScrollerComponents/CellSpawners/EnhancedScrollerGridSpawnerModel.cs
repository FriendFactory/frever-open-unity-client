using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;

namespace UIManaging.EnhancedScrollerComponents.CellSpawners
{
    public class EnhancedScrollerGridSpawnerModel<M>
    {
        public event Action<List<M>> OnItemsChangedEvent;
        public event Action<List<M>, bool> OnItemsAddedEvent;

        public List<M> Items { get; private set; }

        public EnhancedScrollerGridSpawnerModel() { }

        public EnhancedScrollerGridSpawnerModel(IEnumerable<M> items)
        {
            Items = new List<M>(items);
        }

        public void SetItems(IEnumerable<M> items)
        {
            Items = items.ToList();
            OnItemsChangedEvent?.Invoke(Items);
        }

        public void AddItems(IEnumerable<M> items, bool append)
        {
            var list = items.Except(Items).ToList();

            if (append || Items.Count == 0)
            {
                Items.AddRange(list);
            }
            else
            {
                Items.InsertRange(0, list);
            }
            
            OnItemsAddedEvent?.Invoke(list, append);
        }
    }
}