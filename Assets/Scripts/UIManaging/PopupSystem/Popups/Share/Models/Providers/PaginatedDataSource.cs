using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abstract;
using Bridge;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.Share
{
    internal abstract class PaginatedDataSource<TModel>: IAsyncInitializable
    {
        private const int TAKE = 200;
        private static DateTime CacheExpirationTime;
        protected virtual TimeSpan ExpirationCacheTimeSpan { get; } = TimeSpan.FromMinutes(3);

        public bool IsInitialized { get; set; }
        public string SearchQuery { get; private set; }
        public int PageNumber { get; private set; }
        public int PageSize { get; }
        public bool IsLastPageLoaded { get; private set; }

        protected readonly IBridge Bridge;
        
        protected static List<TModel> Models;
        protected List<TModel> FilteredModels;

        protected PaginatedDataSource(IBridge bridge, int pageSize)
        {
            Bridge = bridge;
            PageSize = pageSize;
        }

        public async Task InitializeAsync(CancellationToken token)
        {
            await LoadAllModelsAsync(token);
            
            if (token.IsCancellationRequested) return;

            IsInitialized = true;
        }

        public void CleanUp()
        {
            IsInitialized = false;
        }

        public TModel[] GetFirstPage(string searchQuery = null)
        {
            PageNumber = 1;
            SearchQuery = searchQuery;
            FilteredModels = string.IsNullOrEmpty(searchQuery) ? Models : GetFilteredModels(SearchQuery);
            
            var page = FilteredModels.Take(PageSize).ToArray();

            PageNumber += 1;
            IsLastPageLoaded = page.Length < PageSize || page.Length == FilteredModels.Count; 

            return page;
        }

        public TModel[] GetNextPage()
        {
            // Calculate offset and limit based on page number and size
            var offset = (PageNumber - 1) * PageSize;
            var limit = PageSize;

            // Retrieve the specified page of data
            var page = FilteredModels.Skip(offset).Take(limit).ToArray();

            PageNumber += 1;
            IsLastPageLoaded = page.Length < PageSize || page.Length == FilteredModels.Count; 

            return page;
        }

        protected abstract List<TModel> GetFilteredModels(string searchQuery);
        protected abstract Task<PageResult<TModel>> GetModelsInternal(int take, int skip, CancellationToken token);

        private async Task LoadAllModelsAsync(CancellationToken token)
        {
            if (DateTime.UtcNow < CacheExpirationTime) return;
            
            Models = new List<TModel>();
            var skip = 0;

            while (true)
            {
                var result = await GetModelsInternal(TAKE, skip, token);
                if (result.IsError)
                {
                    Debug.LogError($"[{GetType().Name}] Failed to get models # {result.ErrorMessage}");
                    break;
                }
                
                if (token.IsCancellationRequested) return;
                
                Models.AddRange(result.Models);
                
                if (result.Models.Length < TAKE) break;

                skip += TAKE;
            }

            CacheExpirationTime = DateTime.UtcNow + ExpirationCacheTimeSpan;
        }
    }
}