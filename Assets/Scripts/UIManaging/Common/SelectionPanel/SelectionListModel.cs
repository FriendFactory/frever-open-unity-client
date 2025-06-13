using System;
using System.Collections.Generic;
using System.Linq;

namespace UIManaging.Common.SelectionPanel
{
    public abstract class SelectionListModel<TSelectionItemModel>: ISelectionListModel<TSelectionItemModel> 
        where TSelectionItemModel: class, ISelectionItemModel
    {
        public int MaxSelected { get; }
        public IReadOnlyList<TSelectionItemModel> Items => _items;
        public IReadOnlyList<TSelectionItemModel> SelectedItems => _selectedItems;
        
        public event Action<TSelectionItemModel> ItemSelectionChanged;

        protected readonly List<TSelectionItemModel> _items = new List<TSelectionItemModel>();
        protected readonly List<TSelectionItemModel> _selectedItems = new List<TSelectionItemModel>();

        private readonly Dictionary<TSelectionItemModel, Action> _actions =
            new Dictionary<TSelectionItemModel, Action>();

        protected SelectionListModel(int maxSelected, ICollection<TSelectionItemModel> defaultItems, 
            ICollection<long> selectedIds, ICollection<long> lockedIds)
        {
            MaxSelected = maxSelected;
            AddItems(defaultItems);

            foreach (var item in defaultItems)
            {
                if (selectedIds.Contains(item.Id))
                {
                    item.IsSelected = true;
                }
                
                if (lockedIds.Contains(item.Id))
                {
                    item.IsLocked = true;
                }
            }
        }

        public void AddItems(ICollection<TSelectionItemModel> items)
        {
            var newItems = items.Where(item => _items.All(existingItem => item.Id != existingItem.Id)).ToArray();
            
            _items.AddRange(newItems);

            foreach (var item in newItems)
            {
                _actions[item] = () => OnSelectionChanged(item);
                item.SelectionChanged += _actions[item];
            }
        }

        public void Clear()
        {
            foreach (var item in _items)
            {
                item.SelectionChanged -= _actions[item];
                _actions.Remove(item);
            }
            
            _items.Clear();
        }

        private void OnSelectionChanged(TSelectionItemModel item)
        {
            if (SelectedItems.Count > MaxSelected)
            {
                item.IsSelected = !item.IsSelected;
                return;
            }
            
            if (item.IsSelected && !_selectedItems.Contains(item))
            {
                _selectedItems.Add(item);
                ItemSelectionChanged?.Invoke(item);
            }

            if (!item.IsSelected && _selectedItems.Contains(item))
            {
                _selectedItems.Remove(item);
                ItemSelectionChanged?.Invoke(item);
            }
        }
    }

    public sealed class SelectionListModel: SelectionListModel<ISelectionItemModel>
    {
        public SelectionListModel(int maxSelected, ICollection<ISelectionItemModel> defaultItems, 
            ICollection<long> selectedIds, ICollection<long> lockedIds) : base(maxSelected, defaultItems, selectedIds, lockedIds) { }
    }
}