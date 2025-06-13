using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Results;
using UIManaging.Pages.Common.SongOption.SongList;

namespace UIManaging.Pages.Common.SongOption.Search
{
    public abstract class MusicSearchListModel<T, M>: ISearchListModel<T> where T : PlayableItemModel
    {
        protected readonly IMusicBridge _bridge;
        protected readonly List<T> _models = new List<T>();
        protected readonly int _defaultPageSize;

        public IReadOnlyList<T> Models => _models;

        public bool AwaitingData { get; private set; }
        public bool FirstPageLoaded { get; private set; }
        public bool LastPageLoaded { get; protected set; }
        public string SearchQuery { get; private set; }

        public event Action DataChanged; 
        public event Action FetchFailed;

        public MusicSearchListModel(IMusicBridge bridge, int searchPageSize = 10)
        {
            _bridge = bridge;
            _defaultPageSize = searchPageSize;
        }

        public virtual async Task SearchAsync(string searchQuery, CancellationToken token)
        {
            if (string.IsNullOrEmpty(searchQuery)) return;
            
            if (AwaitingData) return;
            
            AwaitingData = true;
            
            Invalidate();

            var firstPage = await LoadPage(searchQuery, _defaultPageSize, 0, token);
            
            AwaitingData = false;
            
            if (token.IsCancellationRequested) return;

            if (firstPage.IsError)
            {
                Debug.LogWarning($"[{GetType().Name}] Failed to perform search: {firstPage.ErrorMessage}");
                FetchFailed?.Invoke();
                return;
            }

            var modelsCount = firstPage.Models.Length;
            SearchQuery = searchQuery;
            FirstPageLoaded = true;
            LastPageLoaded = modelsCount < _defaultPageSize;

            if (modelsCount == 0)
            {
                DataChanged?.Invoke();
                return;
            }

            AwaitingData = true;
            
            if (await ProcessPage(firstPage, token))
            {
                DataChanged?.Invoke();
            }
            
            AwaitingData = false;
        }

        public async Task SearchNextPageAsync(CancellationToken token)
        {
            if (AwaitingData || LastPageLoaded) return;

            if (string.IsNullOrEmpty(SearchQuery))
            {
                Debug.LogError($"[{GetType().Name}] Failed to search for next page - last search query is empty");
                return;
            }
            
            AwaitingData = true;

            var pageIndex = (Models.Count + _defaultPageSize - 1) / _defaultPageSize;
            var skip = pageIndex * _defaultPageSize;
            
            var nextPage = await LoadPage(SearchQuery, _defaultPageSize, skip, token);
            
            AwaitingData = false;

            if (token.IsCancellationRequested) return;

            if (nextPage.IsError)
            {
                Debug.LogError($"[{GetType().Name}] Failed to perform search # {nextPage.ErrorMessage}");
                FetchFailed?.Invoke();
                return;
            }
            
            var modelsCount = nextPage.Models.Length;
            LastPageLoaded = modelsCount < _defaultPageSize;
            
            if (modelsCount == 0)
            {
                DataChanged?.Invoke();
            }
            
            AwaitingData = true;
            
            if (await ProcessPage(nextPage, token))
            {
                DataChanged?.Invoke();
            }
            
            AwaitingData = false;
        }

        protected void InvokeFetchFailed()
        {
            FetchFailed?.Invoke();
        }
        
        private void Invalidate()
        {
            SearchQuery = string.Empty;
            FirstPageLoaded = false;
            LastPageLoaded = false;
            
            _models.Clear();
            DataChanged?.Invoke();
        }

        protected abstract Task<ArrayResult<M>> LoadPage(string searchQuery = "", int takeNext = 10, int skip = 0,
            CancellationToken token = default);

        protected abstract Task<bool> ProcessPage(ArrayResult<M> page, CancellationToken token);
    }
}