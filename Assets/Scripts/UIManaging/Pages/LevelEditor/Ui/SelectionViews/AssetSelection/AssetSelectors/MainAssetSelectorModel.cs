using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Models.Common;
using UIManaging.Localization;
using UIManaging.Pages.Common.TabsManager;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.PaginationLoaders;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors
{
    public abstract class MainAssetSelectorModel : AssetSelectorModel
    {
        [Inject] protected LevelEditorAssetSelectorLocalization _localization;
            
        protected const int MY_ASSETS_TAB_INDEX = -1;
        protected const int RECOMMENDED_TAB_INDEX = -2;
        protected const string ASSET_SEARCH_REGEXP_FORMAT = "\\b(?=\\w){0}";

        public event Action OnUpdateRequestValues;
        public event Action OnInterruptDownload;

        protected readonly ICategory[] Categories;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public AssetSelectorModel SubSelector { get; set; }
        public AssetCategoryTabsManagerArgs TabsManagerArgs { get; protected set; }

        public virtual bool IsStartOfScroll(long categoryId) => true;
        public virtual bool IsEndOfScroll(long categoryId) => true;
        
        public virtual string NoAvailableAssetsMessage => _localization.AssetSearchEmptyPlaceholder;

        public bool IsMyAssetsCategory => ShowMyAssetsCategory && TabsManagerArgs.SelectedTabIndex == MY_ASSETS_TAB_INDEX;

        public bool IsRecommendedCategory => ShowRecommendedCategory && TabsManagerArgs.SelectedTabIndex == RECOMMENDED_TAB_INDEX;
        public bool RequestScrollPositionUpdate { get; set; }

        protected bool ShowMyAssetsCategory { get; }
        protected bool ShowRecommendedCategory { get; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
    
        protected MainAssetSelectorModel(string displayName, ICategory[] categories = null, bool showMyAssetsCategory = false, bool showRecommendedCategory = false) : 
            base(displayName)
        {
            Categories = categories;
            ShowMyAssetsCategory = showMyAssetsCategory;
            ShowRecommendedCategory = showRecommendedCategory;
            SetupTabManagerArgs();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void UpdateRequestValues()
        {
            OnUpdateRequestValues?.Invoke();
        }

        public virtual void SetDefaultPageSize(int size) { }

        public virtual void SetFilter(string filter = null) { }

        public virtual void SetStartingItem(long categoryId, long? itemId = null) { }

        public virtual Task DownloadInitialPage(long categoryId, bool force = false, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        public virtual void DownloadFirstPage(long categoryId) { }

        public virtual void DownloadNextPage(long categoryId) { }

        public void InterruptDownload()
        {
            OnInterruptDownload?.Invoke();
        }
        
        public virtual List<AssetSelectionItemModel> GetItemsToShow(PaginationLoaderType type, long? categoryId = null, string filter = null)
        {
            switch (type)
            {
                case PaginationLoaderType.Category:
                    var categoryIdStr = PaginationLoaderHelpers.LoaderTypeToString(PaginationLoaderType.Category, categoryId);
            
                    return GetItemsToShow().Where(itm => itm.CategoryId == categoryId || categoryId < 0)
                                           .OrderBy(itm => itm.ItemIndices.ContainsKey(categoryIdStr) ? itm.ItemIndices[categoryIdStr] : -1)
                                           .ToList();
                case PaginationLoaderType.Search:
                    var regex = new Regex(string.Format(ASSET_SEARCH_REGEXP_FORMAT, Regex.Escape(filter)), 
                                          RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    
                    return GetItemsToShow().Where(itm => regex.IsMatch(itm.DisplayName))
                                           .OrderBy(itm => itm.ItemIndices.ContainsKey(filter) ? itm.ItemIndices[filter] : -1)
                                           .ToList();
                case PaginationLoaderType.MyAssets:
                    var myAssetsCategory = PaginationLoaderHelpers.LoaderTypeToString(PaginationLoaderType.MyAssets);
            
                    return GetItemsToShow().Where(itm => itm.IsPurchased)
                                           .OrderBy(itm => itm.ItemIndices.ContainsKey(myAssetsCategory) ? itm.ItemIndices[myAssetsCategory] : -1)
                                           .ToList();
                case PaginationLoaderType.Recommended:
                    var recommendedCategory = PaginationLoaderHelpers.LoaderTypeToString(PaginationLoaderType.Recommended);
            
                    return GetItemsToShow().OrderBy(itm => itm.ItemIndices.ContainsKey(recommendedCategory) ? itm.ItemIndices[recommendedCategory] : -1)
                                           .ToList();
                default:
                    Debug.LogError($"Unknown pagination loader type {type}");
                    return null;
            }
        }

        public virtual void SetTabToSelection(bool allowAutoSwitchToRecommended = false)
        {
            if (AssetSelectionHandler.SelectedModels.Count == 0)
            {
                return;
            }
           
            TabsManagerArgs.SetSelectedTabIndexSilent((int)AssetSelectionHandler.SelectedModels[0].CategoryId);
        }

        public void UpdateTabs()
        {
            TabsManagerArgs.SetSelectedTabIndex(TabsManagerArgs.SelectedTabIndex);
        }

        public int GetSelectedItemIndexInCategory()
        {
            if (AssetSelectionHandler.SelectedModels.Count == 0)
            {
                return 0;
            }
            
            var items = GetItemsToShow(PaginationLoaderType.Category, TabsManagerArgs.SelectedTabIndex);
            var selectedItemIndex = items.IndexOf(AssetSelectionHandler.SelectedModels[0]);
            
            return selectedItemIndex;
        }
        
        public long GetSelectedItemIdInCategory()
        {
            if (AssetSelectionHandler.SelectedModels.Count == 0)
            {
                return -1;
            }
            
            var items = GetItemsToShow(IsRecommendedCategory 
                                           ? PaginationLoaderType.Recommended 
                                           : PaginationLoaderType.Category, TabsManagerArgs.SelectedTabIndex);
            
            return items.Contains(AssetSelectionHandler.SelectedModels[0]) ? AssetSelectionHandler.SelectedModels[0].ItemId : -1;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected virtual int GetInitialTabIndex(TabModel[] tabModels) => tabModels[0].Index;

        protected virtual TabModel[] GetTabs()
        {
            return Categories.Select(CreateTabModel).ToArray();
        }

        protected virtual void SetupTabManagerArgs()
        {
            var tabs = GetTabs();
            var initialIndex = tabs.Any() ? GetInitialTabIndex(tabs) : 0;
            
            TabsManagerArgs = new AssetCategoryTabsManagerArgs(tabs, initialIndex, ShowMyAssetsCategory, () => ShowRecommendedCategory);
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static TabModel CreateTabModel(ICategory category)
        {
            if (category is IAssetCategory assetCategory)
            {
                return new TabModel(Convert.ToInt32(category.Id), category.Name, false, assetCategory.HasNew);
            }
            return new TabModel(Convert.ToInt32(category.Id), category.Name);
        }
    }
}
