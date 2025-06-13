using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Models.Common;
using UIManaging.Pages.Common.TabsManager;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.PaginationLoaders;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.AssetSelectors
{
    public class PaginatedAssetSelectorParameters<TLoader, TModel> 
        where TLoader: AssetSelectorPaginationLoader<TModel> 
        where TModel: class, IEntity
    {
        public string DisplayName;
        public ICategory[] Categories;
        public Func<long, TLoader> LoaderCreator;
        public Func<string, TLoader> SearchLoaderCreator;
        public TLoader MyAssetsLoader = null;
        public TLoader RecommendedLoader = null;
        public bool ShowMyAssetsCategory = false;
        public bool ShowRecommendedCategory = false;
    }
    
    public abstract class PaginatedAssetSelectorModel<TLoader, TModel> : MainAssetSelectorModel 
        where TLoader: AssetSelectorPaginationLoader<TModel> 
        where TModel: class, IEntity
    {
        protected IDictionary<long, TLoader> Loaders;
        protected Func<string, TLoader> SearchLoaderCreator;
        protected readonly TLoader MyAssetsLoader;
        protected readonly TLoader RecommendedLoader;

        protected TLoader SearchLoader;

        private int _defaultPageSize;
        
        public override bool AwaitingData 
        {
            get
            {
                if (SearchLoader != null)
                {
                    return SearchLoader.AwaitingData;
                }

                return Loaders.Values.Any(loader => loader.AwaitingData) 
                    || (MyAssetsLoader?.AwaitingData ?? false) 
                    || (RecommendedLoader?.AwaitingData ?? false);
            }
        }
        
        protected PaginatedAssetSelectorModel(PaginatedAssetSelectorParameters<TLoader, TModel> assetSelectorParameters) 
            : base(assetSelectorParameters.DisplayName, assetSelectorParameters.Categories, 
                   assetSelectorParameters.ShowMyAssetsCategory, assetSelectorParameters.ShowRecommendedCategory)
        {
            Loaders = assetSelectorParameters.Categories.ToDictionary(category => category.Id, category =>
            {
                var loader = assetSelectorParameters.LoaderCreator.Invoke(category.Id);

                loader.OnIndexReset += () => RemoveIndicesForCategory(category.Id);
                
                return loader;
            });
            SearchLoaderCreator = assetSelectorParameters.SearchLoaderCreator;
            MyAssetsLoader = assetSelectorParameters.MyAssetsLoader;
            RecommendedLoader = assetSelectorParameters.RecommendedLoader;

            if (MyAssetsLoader != null)
            {
                MyAssetsLoader.OnIndexReset += RemoveIndicesForMyAssets;
            }

            if (RecommendedLoader != null)
            {
                RecommendedLoader.OnIndexReset += RemoveIndicesForRecommended;
            }
        }

        private void RemoveIndicesForCategory(long categoryId)
        {
            var categoryIdStr = PaginationLoaderHelpers.LoaderTypeToString(PaginationLoaderType.Category, categoryId);

            foreach (var item in AllItems.Where(item2 => item2.ItemIndices.ContainsKey(categoryIdStr)))
            {
                item.ItemIndices.Remove(categoryIdStr);
            }
        }
        
        private void RemoveIndicesForMyAssets()
        {
            var myAssetsCategory = PaginationLoaderHelpers.LoaderTypeToString(PaginationLoaderType.MyAssets);

            foreach (var item in AllItems.Where(item2 => item2.ItemIndices.ContainsKey(myAssetsCategory)))
            {
                item.ItemIndices.Remove(myAssetsCategory);
            }
        }
        
        private void RemoveIndicesForRecommended()
        {
            var recommendedCategory = PaginationLoaderHelpers.LoaderTypeToString(PaginationLoaderType.Recommended);

            foreach (var item in AllItems.Where(item2 => item2.ItemIndices.ContainsKey(recommendedCategory)))
            {
                item.ItemIndices.Remove(recommendedCategory);
            }
        }
        
        private void RemoveIndicesForSearch()
        {
            if (SearchLoader == null)
            {
                return;
            }

            var searchFilter = PaginationLoaderHelpers.LoaderTypeToString(PaginationLoaderType.Search, filter: SearchLoader.Filter);
            
            foreach (var item in AllItems.Where(item2 => item2.ItemIndices.ContainsKey(searchFilter)))
            {
                item.ItemIndices.Remove(searchFilter);
            }
        }

        public override void SetTabToSelection(bool allowAutoSwitchToRecommended = false)
        {
            if (AssetSelectionHandler.SelectedModels.Count == 0)
            {
                return;
            }

            // check if ShowRecommendedCategory is true and MovementTypeId isn't null (already done for tab manager args)
            if (TabsManagerArgs.ShowRecommendedCategory && allowAutoSwitchToRecommended)
            {
                // find items designated to recommended tab
                var recommendedIndex = PaginationLoaderHelpers.LoaderTypeToString(PaginationLoaderType.Recommended);
                var recommendedItems = AllItems.Where(item => item.ItemIndices.ContainsKey(recommendedIndex)).ToArray(); 
                
                // if there are no items there, we've never requested recommended tab initial page, so we do it
                // if we already have initial page for recommended tab and selected item is there - keep that tab
                if (recommendedItems.Length == 0 || 
                    recommendedItems.Any(item => item.ItemId == AssetSelectionHandler.SelectedModels[0].ItemId)) 
                {
                    TabsManagerArgs.SetSelectedTabIndexSilent(AssetCategoryTabsManagerArgs.RECOMMENDED_TAB_INDEX);
                    return;
                }
                // otherwise switch to respective category tab
            }
            
            base.SetTabToSelection(allowAutoSwitchToRecommended);
        }

        public override bool IsStartOfScroll(long categoryId) 
        {
            return GetCurrentLoader(categoryId)?.StartOfScroll ?? true;
        }
        
        public override bool IsEndOfScroll(long categoryId) 
        {
            return GetCurrentLoader(categoryId)?.EndOfScroll ?? true;
        }

        public override void SetDefaultPageSize(int size)
        {
            _defaultPageSize = size;
            SearchLoader?.SetDefaultPageSize(size);
            MyAssetsLoader?.SetDefaultPageSize(size);
            RecommendedLoader?.SetDefaultPageSize(size);

            foreach (var loader in Loaders.Values)
            {
                loader.SetDefaultPageSize(size);
            }
        }
        
        public override void SetFilter(string filter = null)
        {
            if (string.IsNullOrEmpty(filter))
            {
                if (SearchLoader == null)
                {
                    return;
                }

                var searchFilter = PaginationLoaderHelpers.LoaderTypeToString(PaginationLoaderType.Search, filter: SearchLoader.Filter);
                
                foreach (var item in AllItems.Where(item => item.ItemIndices.ContainsKey(searchFilter)))
                {
                    item.ItemIndices.Remove(searchFilter);
                }

                SearchLoader.OnIndexReset -= RemoveIndicesForSearch;
                SearchLoader = null;
                return;
            }

            if (SearchLoader != null)
            {
                var searchFilter = PaginationLoaderHelpers.LoaderTypeToString(PaginationLoaderType.Search, filter: SearchLoader.Filter);
                
                foreach (var item in AllItems.Where(item => item.ItemIndices.ContainsKey(searchFilter)))
                {
                    item.ItemIndices.Remove(searchFilter);
                }
                
                SearchLoader.OnIndexReset -= RemoveIndicesForSearch;
            }
            
            SearchLoader = SearchLoaderCreator?.Invoke(filter);

            if (SearchLoader != null)
            {
                SearchLoader.SetDefaultPageSize(_defaultPageSize);
                SearchLoader.OnIndexReset += RemoveIndicesForSearch;
            }
        }
        
        public sealed override async Task DownloadInitialPage(long categoryId, bool force = false, CancellationToken token = default)
        {
            await GetCurrentLoader(categoryId).DownloadInitialPage(force, token);
        }
        
        public sealed override void DownloadFirstPage(long categoryId)
        {
            GetCurrentLoader(categoryId)?.DownloadFirstPage();
        }

        public sealed override void DownloadNextPage(long categoryId)
        {
            GetCurrentLoader(categoryId)?.DownloadNextPage();
        }

        public override void SetStartingItem(long categoryId, long? itemId = null)
        {
            GetCurrentLoader(categoryId)?.SetStartingItem(itemId);
        }
        
        public override void SetSelectedItems(long[] itemIds = null, long[] categoryIds = null,
                                                    bool silent = false)
        {
            if (itemIds == null || categoryIds == null || itemIds.Length != categoryIds.Length)
            {
                base.SetSelectedItems(itemIds, categoryIds, silent);
                return;
            }

            RequestScrollPositionUpdate = true;
            
            for (var i = 0; i < itemIds.Length; i++)
            {
                if (Loaders.ContainsKey(categoryIds[i]))
                {
                    Loaders[categoryIds[i]].SetStartingItem(itemIds[i]);
                }
            }

            if (itemIds.Length > 0 && ShowRecommendedCategory)
            {
                RecommendedLoader.SetStartingItem(itemIds[0]);
            }
            
            base.SetSelectedItems(itemIds, categoryIds, silent);
        }

        public override List<AssetSelectionItemModel> GetItemsToShow(PaginationLoaderType type, long? categoryId = null, string filter = null)
        {
            var indexStr = PaginationLoaderHelpers.LoaderTypeToString(type, categoryId, filter);
            var allowedElementsAmount = GetAllowedElementsAmount(type, categoryId, filter);
            
            return base.GetItemsToShow(type, categoryId, filter)
                       .Where(item => item.ItemIndices.ContainsKey(indexStr) 
                                   && item.ItemIndices[indexStr] >= 0 
                                   && item.ItemIndices[indexStr] < allowedElementsAmount)
                       .ToList();
        }

        private int GetAllowedElementsAmount(PaginationLoaderType type, long? categoryId = null, string filter = null)
        {
            switch (type)
            {
                case PaginationLoaderType.Category:
                    return !categoryId.HasValue || !Loaders.ContainsKey(categoryId.Value) ? 0 : Loaders[categoryId.Value].Models.Count;
                case PaginationLoaderType.Search:
                    return SearchLoader == null ? 0 : SearchLoader.Models.Count;
                case PaginationLoaderType.MyAssets:
                    return MyAssetsLoader != null && IsMyAssetsCategory ? MyAssetsLoader.Models.Count : 0;
                case PaginationLoaderType.Recommended:
                    return RecommendedLoader != null && IsRecommendedCategory ? RecommendedLoader.Models.Count : 0;
                default:
                    Debug.LogError($"Unknown pagination loader type: {type}");
                    return 0;
            }
        }

        protected TLoader GetCurrentLoader(long categoryId)
        {
            if (SearchLoader != null)
            {
                return SearchLoader;
            }
            
            if (MyAssetsLoader != null && IsMyAssetsCategory)
            {
                return MyAssetsLoader;
            }
            
            if (RecommendedLoader != null && IsRecommendedCategory)
            {
                return RecommendedLoader;
            }
            
            if (!Loaders?.ContainsKey(categoryId) ?? true)
            {
                return null;
            }

            return Loaders[categoryId];
        }

        protected override void OnIsSelectedChangedByUser(AssetSelectionItemModel itemModel)
        {
            if (Loaders.ContainsKey(itemModel.CategoryId))
            {
                Loaders[itemModel.CategoryId].SetStartingItemSilent(itemModel.ItemId);
            }

            if (ShowRecommendedCategory)
            {
                RecommendedLoader.SetStartingItemSilent(itemModel.ItemId);
            }
            
            base.OnIsSelectedChangedByUser(itemModel);
        }
    }
}
