using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.ExternalPackages.AsynAwaitUtility;
using Bridge.Models.Common;
using Bridge.Results;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using UIManaging.Common.Loaders;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.PaginationLoaders
{
    public abstract class AssetSelectorPaginationLoader<TModel>: GenericPaginationLoader<TModel> where TModel: class, IEntity
    {
        protected override object LastLoadedItemId => Models.Count > 0 ? Models[Models.Count-1].Id : StartId;
        protected override object FirstLoadedItemId => Models.Count > 0 ? Models[0].Id : StartId;
        
        protected override int DefaultPageSize => _defaultPageSize + 1; // get an additional item to find out if it's the last page

        private int _defaultPageSize = 8;

        internal PaginationLoaderType Type { get; }
        internal bool StartOfScroll { get; private set; }
        internal bool EndOfScroll { get; private set; }
        internal long? StartId { get; private set; }
        internal long CategoryId { get; }
        internal string Filter { get; }
        internal bool SearchInAllCategories { get; }
        private bool IsLoadingInitialPageRunning { get; set; }
        protected long? TaskId { get; }
        protected long UniverseId { get; }
        protected Func<AssetSelectorModel> SelectorModel { get; }
        
        [Inject, UsedImplicitly] protected IBridge Bridge { get; private set; }
        [Inject, UsedImplicitly] protected IDataFetcher DataFetcher { get; private set; }

        public event Action NewPageInitialized;
        public event Action OnIndexReset;
        public event Action<ICollection<TModel>> OnPageLoadedEvent;
        
        internal AssetSelectorPaginationLoader(PaginationLoaderType type, Func<AssetSelectorModel> selectorModel, long universeId, long? categoryId = null, string filter = null, long? taskId = null)
        {
            SelectorModel = selectorModel;
            Filter = filter;
            SearchInAllCategories = !categoryId.HasValue;
            Type = type;
            UniverseId = universeId;
            TaskId = taskId;
            
            if (categoryId.HasValue)
            {
                CategoryId = categoryId.Value;
            }
        }

        public void SetDefaultPageSize(int defaultPageSize)
        {
            _defaultPageSize = defaultPageSize;
        }

        public async Task DownloadInitialPage(bool force = false, CancellationToken token = default)
        {
            if (Models.Count > 0 && !IsLoadingInitialPageRunning && !force)
            {
                return;
            }
            
            if (!IsLoadingInitialPageRunning)
            {
                await DownloadInitialPageInternal(token);
            }
            else
            {
                await new WaitWhile(() => IsLoadingInitialPageRunning);
            }
        }

        private async Task DownloadInitialPageInternal(CancellationToken token)
        {
            IsLoadingInitialPageRunning = true;
            AwaitingData = true;
            StartOfScroll = false;
            EndOfScroll = false;
            
            Models.Clear();
            OnIndexReset?.Invoke();

            var models = await DownloadModelsInternal(StartId, PageSize, DefaultPageSize, token);

            if (models == null)
            {
                IsLoadingInitialPageRunning = false;
                AwaitingData = false;
                return;
            }
            
            Models.AddRange(models);
            OnPageLoaded(models, true);
            NewPageInitialized?.Invoke();
            
            IsLoadingInitialPageRunning = false;
            AwaitingData = false;
        }

        public void SetStartingItem(long? startId = null)
        {
            if (startId != null && Models.All(model => model.Id != startId))
            {
                Reset();
            }
            
            StartId = startId;
        }

        public void SetStartingItemSilent(long? startId = null)
        {
            StartId = startId;
        }

        public override void Reset()
        {
            StartOfScroll = false;
            EndOfScroll = false;

            Models.Clear();
            OnIndexReset?.Invoke();
        }

        protected override void OnNextPageLoaded(TModel[] page)
        {
            OnPageLoaded(page, false, true);
        }

        protected override void OnFirstPageLoaded(TModel[] page)
        {
            OnPageLoaded(page, false, false);
        }

        private void OnPageLoaded(IList<TModel> page, bool initial, bool append = true)
        {
            page = page.ToList();
            
            var selectedIndex = StartId == 0 ? 0 : page.IndexOf(page.FirstOrDefault(model => model.Id == StartId));
            var startOffset = initial && selectedIndex > _defaultPageSize // if there's full page of items before the starting item - we're not at the scroll start
                           || !initial && !append && page.Count > _defaultPageSize; // if there's full page of items prepending - we're not at the scroll start
            var endOffset = initial && page.Count - selectedIndex > _defaultPageSize // if there's full page of items after the starting item - we're not at the scroll end
                         || !initial && append && page.Count > _defaultPageSize; // if there's full page of items appending - we're not at the scroll end
            
            if (startOffset) // if we're not at the scroll start - remove additional items from the loader
            {
                Models.RemoveAt(0);
                page.RemoveAt(0);
            }
            else if (initial || !append) // otherwise if we weren't appending this page, it means we've reached the start of the scroll
            {
                StartOfScroll = true;
            }
            
            if (endOffset) // if we're not at the scroll end - remove additional items from the loader
            {
                Models.RemoveAt(Models.Count - 1);
                page.RemoveAt(page.Count - 1);
            }
            else if (initial || append) // otherwise if we weren't prepending this page, it means we've reached the end of the scroll
            {
                EndOfScroll = true;
            }
            
            var itemModels = CreateItemModels(page);
            var indexKey = PaginationLoaderHelpers.LoaderTypeToString(Type, SearchInAllCategories ? null : (long?)CategoryId, Filter);
            
            foreach (var item in itemModels)
            {
                item.ItemIndices[indexKey] = Models.IndexOf(item.RepresentedObject as TModel);
            }

            if (!append) // if we're prepending the page - move indexes of all other elements
            {
                foreach (var item in SelectorModel.Invoke().Models)
                {
                    if (item.ItemIndices.ContainsKey(indexKey))
                    {
                        item.ItemIndices[indexKey] += itemModels.Count;
                    }
                }
            }

            if (SelectorModel != null)
            {
                SelectorModel.Invoke()?.AddItems(itemModels, initial, append);
            }
            OnPageLoadedEvent?.Invoke(page);
        }
        
        protected sealed override async Task<TModel[]> DownloadModelsInternal(object targetId, int takeNext, int takePrevious = 0, CancellationToken token = default)
        {
            var categoryId = SearchInAllCategories ? (long?)null : CategoryId;
            var withAnimations = categoryId is < 0;
            var result = await GetAssetListAsync((long?) targetId, takeNext, takePrevious, categoryId, Filter, token);

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                Debug.LogError(result.ErrorMessage);
                return null;
            }

            if (result.IsSuccess)
            {
                return result.Models;
            }

            return null;
        }

        protected abstract Task<ArrayResult<TModel>> GetAssetListAsync(long? targetId, int takeNext, int takePrevious, long? categoryId, string filter, CancellationToken token);

        protected abstract IList<AssetSelectionItemModel> CreateItemModels(IList<TModel> page);
    }
}