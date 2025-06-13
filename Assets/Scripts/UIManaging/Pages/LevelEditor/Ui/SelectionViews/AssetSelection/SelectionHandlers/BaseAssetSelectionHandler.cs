using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.Common;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.SelectionHandlers
{
    public abstract class BaseAssetSelectionHandler
    {
        public event Action OnAmountOfSelectedItemsChangedEvent;
        public IReadOnlyList<AssetSelectionItemModel> SelectedModels => _selectedItemModels;
        
        protected readonly Dictionary<long, AssetSelectionItemModel> UniqueSelectedItemModels = new Dictionary<long, AssetSelectionItemModel>();

        protected IReadOnlyList<AssetSelectionItemModel> AllItems { get; private set; }
        protected int MinSelectedItemsAmount { get; }
        protected int MaxSelectedItemsAmount { get; }

        protected bool CanRefreshSelection { get; }
        protected Action<IEntity> ExtraItemSelectionChangeAction { get; private set; }

        private List<AssetSelectionItemModel> _selectedItemModels;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        protected BaseAssetSelectionHandler(int minSelectedItemsAmount, int maxSelectedItemsAmount, bool canRefreshSelection)
        {
            MinSelectedItemsAmount = minSelectedItemsAmount;
            MaxSelectedItemsAmount = maxSelectedItemsAmount;
            CanRefreshSelection = canRefreshSelection;
            _selectedItemModels = new List<AssetSelectionItemModel>();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public abstract void ChangeItemSelection(AssetSelectionItemModel itemModel, bool value);

        public void SetExtraItemSelectionChangeAction(Action<IEntity> action)
        {
            ExtraItemSelectionChangeAction = action;
        }

        public void SetAllItems(List<AssetSelectionItemModel> items)
        {
            AllItems = items;
        }

        public void SetSelectedItems(IEnumerable<AssetSelectionItemModel> itemModels)
        {
            _selectedItemModels = itemModels.ToList();

            UniqueSelectedItemModels.Clear();
            foreach (var item in _selectedItemModels)
            {
                UniqueSelectedItemModels[item.ItemId] = item;
            }
        }
    
        public void UnselectAllSelectedItemsSilently()
        {
            var selectedItemModels = _selectedItemModels.ToArray();
            
            OnUnSelection();
            
            foreach (var selectedItem in selectedItemModels)
            {
                selectedItem.SetIsSelected(false, true);
            }

        }

        public void UnselectAllSelectedItems()
        {
            var selectedItemModels = _selectedItemModels.ToArray();
            
            OnUnSelection();
            
            foreach (var selectedItem in selectedItemModels)
            {
                selectedItem.SetIsSelected(false);
            }
        }
        
        public bool IsItemAlreadySelected(long itemId, long categoryId)
        {
            return _selectedItemModels.Any(x=> x.ItemId == itemId && x.CategoryId == categoryId);
        }

        public void UnselectSelectedItem(AssetSelectionItemModel itemModel)
        {
            RemoveItem(itemModel);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected void InvokeOnAmountOfSelectedItemsChangedEvent()
        {
            OnAmountOfSelectedItemsChangedEvent?.Invoke();
        }

        protected void RemoveItem(AssetSelectionItemModel itemModel)
        {
            UniqueSelectedItemModels.Remove(itemModel.ItemId);
            _selectedItemModels.Remove(itemModel);
        }
        
        protected void AddItem(AssetSelectionItemModel itemModel)
        {
            UniqueSelectedItemModels[itemModel.ItemId] = itemModel;
            _selectedItemModels.Add(itemModel);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnUnSelection()
        {
            _selectedItemModels.Clear();
            UniqueSelectedItemModels.Clear();
            InvokeOnAmountOfSelectedItemsChangedEvent();
        }
    }
}