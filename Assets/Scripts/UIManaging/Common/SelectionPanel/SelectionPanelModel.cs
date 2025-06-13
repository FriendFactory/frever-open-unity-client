using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;

namespace UIManaging.Common.SelectionPanel
{
    public abstract class SelectionPanelModel<TSelectionItemModel>: ISelectionPanelModel<TSelectionItemModel> 
        where TSelectionItemModel: class, ISelectionItemModel
    {
        public int MaxSelected { get; }
        public IReadOnlyList<TSelectionItemModel> Items => _items;
        public IReadOnlyList<TSelectionItemModel> SelectedItems => _selectedItems;
        
        public event Action<TSelectionItemModel> ItemSelectionChanged;
        public event Action SelectionLimitReached;

        protected readonly List<TSelectionItemModel> _items = new List<TSelectionItemModel>();
        protected readonly List<TSelectionItemModel> _selectedItems = new List<TSelectionItemModel>();

        protected readonly Dictionary<TSelectionItemModel, Action> _actions =
            new Dictionary<TSelectionItemModel, Action>();

        protected SelectionPanelModel(int maxSelected, ICollection<TSelectionItemModel> preselectedItems, ICollection<TSelectionItemModel> lockedItems)
        {
            MaxSelected = maxSelected;
            AddItems(preselectedItems);
            AddItems(lockedItems);
            
            lockedItems?.ForEach(item => item.IsLocked = true);
        }

        public void AddItems(ICollection<TSelectionItemModel> items)
        {
            if (items == null) return;
            
            var newItems = items.Where(item => _items.All(existingItem => item.Id != existingItem.Id)).ToArray();
            
            _items.AddRange(newItems);

            foreach (var item in newItems)
            {
                _actions[item] = () => OnSelectionChanged(item);
                item.SelectionChanged += _actions[item];
            }
            
            _selectedItems.AddRange(newItems.Where(x=>x.IsSelected));
        }

        public void Clear()
        {
            foreach (var item in _items)
            {
                item.SelectionChanged -= _actions[item];
                _actions.Remove(item);
            }
            
            _items.Clear();

            ItemSelectionChanged = null;
        }

        private void OnSelectionChanged(TSelectionItemModel item)
        {
            if (SelectedItems.Count >= MaxSelected && item.IsSelected)
            {
                item.IsSelected = false;
                SelectionLimitReached?.Invoke();
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

    public sealed class SelectionPanelModel: SelectionPanelModel<ISelectionItemModel>
    {
        public SelectionPanelModel(int maxSelected, ICollection<ISelectionItemModel> preselectedItems, 
            ICollection<ISelectionItemModel> lockedItems) : base(maxSelected, preselectedItems, lockedItems) { }
    }
}