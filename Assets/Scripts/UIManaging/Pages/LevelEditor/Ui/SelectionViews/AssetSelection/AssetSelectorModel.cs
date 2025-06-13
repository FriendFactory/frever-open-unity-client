using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using Modules.AssetsStoraging.Core;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.EnhancedScrollerComponents.CellSpawners;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.SelectionHandlers;
using Zenject;
using Event = Models.Event;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection
{
    public abstract class AssetSelectorModel
    {
        protected readonly List<AssetSelectionItemModel> AllItems;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public IReadOnlyList<AssetSelectionItemModel> Models => AllItems.AsReadOnly();
        public abstract DbModelType AssetType { get; }
        public BaseAssetSelectionHandler AssetSelectionHandler { get; private set; }
        public EnhancedScrollerGridSpawnerModel<AssetSelectionItemModel> GridSpawnerModel { get; protected set; }
        public string DisplayName { get; }
        public virtual bool AwaitingData => false;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<AssetSelectionItemModel> OnSelectedItemChangedByUserEvent;
        public event Action<AssetSelectionItemModel> OnSelectedItemChangedEvent;
        public event Action<AssetSelectionItemModel> OnSelectedItemSilentChangedEvent;
        public event Action OnSelectionChangedByCodeEvent;
        public event Action<AssetSelectionItemModel[], bool> ItemsAdded;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        protected AssetSelectorModel(string displayName)
        {
            ProjectContext.Instance.Container.Inject(this);
            DisplayName = displayName;
            AllItems = new List<AssetSelectionItemModel>();
            GridSpawnerModel = new EnhancedScrollerGridSpawnerModel<AssetSelectionItemModel>();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void AddItems(IEnumerable<AssetSelectionItemModel> itemsToAdd, bool initial = false, bool append = true)
        {
            var newItems = new List<AssetSelectionItemModel>();

            var itemsToAddArray = itemsToAdd as AssetSelectionItemModel[] ?? itemsToAdd.ToArray();
            
            foreach (var item in itemsToAddArray)
            {
                var existingItem = AllItems.FirstOrDefault(itm => itm.ItemId == item.ItemId && (itm.CategoryId == item.CategoryId || itm.IsProxy));
                
                if (existingItem == null)
                {
                    newItems.Add(item);
                }
                else if (existingItem.IsProxy)
                {
                    existingItem.UpdateProxy(item);
                } 
                else 
                {
                    foreach (var pair in item.ItemIndices)
                    { 
                       existingItem.ItemIndices[pair.Key] = pair.Value;
                    }
                }
            }
            
            AllItems.AddRange(newItems);

            UnsubscribeToItemEvents(newItems);
            SubscribeToItemEvents(newItems);
            
            if (!initial)
            {
                ItemsAdded?.Invoke(newItems.ToArray(), append);
            }
        }

        public virtual void SetSelectedItemsAsInEvent(ILevelManager levelManager, Event @event,
            IDataFetcher dataFetcher = null,
            bool silent = true) { }

        public virtual bool AreSelectedItemsAsInEvent(ILevelManager levelManager, Event @event)
        {
            return true;
        }

        public virtual List<AssetSelectionItemModel> GetItemsToShow()
        {
            return AllItems;
        }

        protected void SetAssetSelectionHandler(BaseAssetSelectionHandler assetSelectionHandler)
        {
            AssetSelectionHandler = assetSelectionHandler;
            AssetSelectionHandler.SetAllItems(AllItems);
        }

        public virtual void SetSelectedItems(long[] itemIds = null, long[] categoryId = null, bool silent = false)
        {
            if (AssetSelectionHandler == null)
            {
                return;
            }

            AssetSelectionHandler.UnselectAllSelectedItemsSilently();
            
            if (itemIds == null)
            {
                return;
            }
            
            AssetSelectionHandler.SetSelectedItems(AllItems.Where(item => itemIds.Contains(item.ItemId)).ToArray());

            foreach (var item in AllItems)
            {
                if (itemIds.Contains(item.ItemId))
                {
                    item.SetIsSelected(true, silent);
                }
            }
            
            OnSelectionChangedByCodeEvent?.Invoke();
        }

        public virtual bool ShouldShowSpawnFormationPanel()
        {
            return false;
        }

        public virtual bool ShouldShowUploadingPanel(SetLocationFullInfo setLocation)
        {
            return false;
        }

        public virtual bool ShouldShowRevertButton()
        {
            return true;
        }

        public virtual bool ShouldShowCharactersSelectionPanel()
        {
            return false;
        }

        public virtual bool ShouldShowCharactersSwitchablePanel()
        {
            return false;
        }

        public virtual bool ShouldShowCreateNewOutfitPanel()
        {
            return false;
        }

        public virtual bool ShouldShowTimeOfDayJogWheel(ISetLocationAsset setLocation)
        {
            return false;
        }
        
        public virtual bool IsSearchable()
        {
            return true;
        }

        public virtual void OnOpened() { }

        public virtual void OnClosed() { }

        public void LockItems()
        {
            foreach (var item in AllItems)
            {
                item.Lock();
            }
        }

        public void UnlockItems()
        {
            foreach (var item in AllItems)
            {
                item.Unlock();
            }
        }
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SubscribeToItemEvents(IEnumerable<AssetSelectionItemModel> models)
        {
            foreach (var item in models)
            {
                item.OnIsSelectedChangedByUserEvent += OnIsSelectedChangedByUser;
                item.OnIsSelectedChangedEvent += OnIsSelectedChanged;
                item.OnIsSelectedChangedSilentEvent += OnIsSelectedSilentChanged;
            }
        }
        
        private void UnsubscribeToItemEvents(IEnumerable<AssetSelectionItemModel> models)
        {
            foreach (var item in models)
            {
                item.OnIsSelectedChangedByUserEvent -= OnIsSelectedChangedByUser;
                item.OnIsSelectedChangedEvent -= OnIsSelectedChanged;
                item.OnIsSelectedChangedSilentEvent -= OnIsSelectedSilentChanged;
            }
        }

        protected virtual void OnIsSelectedChangedByUser(AssetSelectionItemModel itemModel)
        {
            AssetSelectionHandler.ChangeItemSelection(itemModel, !itemModel.IsSelected);
            
            OnSelectedItemChangedByUserEvent?.Invoke(itemModel);
        }
        
        private void OnIsSelectedChanged(AssetSelectionItemModel itemModel)
        {
            var duplicateItem = GetDuplicateItem(itemModel);
            duplicateItem?.SetIsSelected(itemModel.IsSelected);

            if (!itemModel.IsSelected && AssetSelectionHandler.IsItemAlreadySelected(itemModel.ItemId, itemModel.CategoryId))
            {
                AssetSelectionHandler.UnselectSelectedItem(itemModel);
            }
            
            OnSelectedItemChangedEvent?.Invoke(itemModel);
        }
        
        private void OnIsSelectedSilentChanged(AssetSelectionItemModel itemModel)
        {
            OnSelectedItemSilentChangedEvent?.Invoke(itemModel);
        }

        private AssetSelectionItemModel GetDuplicateItem(AssetSelectionItemModel item)
        {
            return AllItems.FirstOrDefault(x => x.ItemId == item.ItemId && x.CategoryId == item.CategoryId && x.IsSelected != item.IsSelected);
        }
    }
}