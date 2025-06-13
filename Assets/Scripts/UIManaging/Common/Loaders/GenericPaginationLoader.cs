using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Models.Common;
using Extensions;

namespace UIManaging.Common.Loaders
{
    public abstract class GenericPaginationLoader<TModel> where TModel : IEntity
    {
        public event Action LastPageLoaded;
        public event Action NewPagePrepended;
        public event Action NewPageAppended;

        private CancellationTokenSource _cancellationTokenSource;

         //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public List<TModel> Models { get; } = new List<TModel>();
        public bool AwaitingData { get; protected set; }

        protected virtual object LastLoadedItemId => Models?.Count > 0 ? Models[Models.Count-1].Id : (long?) null;
        protected virtual object FirstLoadedItemId => Models?.Count > 0 ? Models[0].Id : (long?) null;
        protected virtual int PrependDataIndex => 0;

        /// <summary>
        /// Item list from response comes with starting item from server, which is +1
        /// </summary>
        protected int PageSize => Models.Count > 0 ? DefaultPageSize + 1 : DefaultPageSize;

        protected abstract int DefaultPageSize { get; }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        /// <summary>
        /// Download page starting with last loaded video and append it to _models list
        /// </summary>
        public virtual async void DownloadNextPage()
        {
            if(AwaitingData) return;
            AwaitingData = true;

            CancelLoading();
            _cancellationTokenSource = new CancellationTokenSource();
            
            var models = await DownloadModelsInternal(LastLoadedItemId, PageSize, token:_cancellationTokenSource.Token);

            if (models == null)
            {
                AwaitingData = false;
                return;
            }
            
            var page = Models.Count > 0 ? models.Skip(1).ToArray() : models;
            if (page.Length == 0)
            {
                LastPageLoaded?.Invoke();
                AwaitingData = false;
                return;
            }
            
            Models.AddRange(page);
            OnNextPageLoaded(page);
            NewPageAppended?.Invoke();
            
            AwaitingData = false;
        }

        /// <summary>
        /// Download page before first loaded and prepend it to _models list
        /// </summary>
        public async void DownloadFirstPage()
        {
            if (Models.Count == 0)
            {
                DownloadNextPage();
                return;
            }
            
            AwaitingData = true;

            CancelLoading();
            _cancellationTokenSource = new CancellationTokenSource();

            var loadedModels = await DownloadModelsInternal(FirstLoadedItemId, 0, DefaultPageSize, _cancellationTokenSource.Token);
            if (loadedModels.IsNullOrEmpty())
            {
                AwaitingData = false;
                return;
            }

            TModel[] page;
            if (loadedModels.Length == 0 || Models.Count == 0)
            {
                page = loadedModels;
            }
            else
            {
                var firstCachedId = Models[0].Id;
                page = loadedModels.Where(item => item.Id != firstCachedId).ToArray();
            }
            Models.InsertRange(PrependDataIndex, page);
            OnFirstPageLoaded(page);
            NewPagePrepended?.Invoke();
            
            AwaitingData = false;
        }

        public virtual void Reset()
        {
            CancelLoading(true);
            Models.Clear();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected void InvokeNewPageAppended()
        {
            NewPageAppended?.Invoke();
        }
        
        protected void InvokeLastPageLoaded()
        {
            LastPageLoaded?.Invoke();
        }

        protected abstract void OnNextPageLoaded(TModel[] page);

        protected abstract void OnFirstPageLoaded(TModel[] page);

        protected abstract Task<TModel[]> DownloadModelsInternal(object targetId, int takeNext, int takePrevious = 0, CancellationToken token = default);

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void CancelLoading(bool dispose = false)
        {
            _cancellationTokenSource?.CancelAndDispose();
            _cancellationTokenSource = null;
        }
    }
}