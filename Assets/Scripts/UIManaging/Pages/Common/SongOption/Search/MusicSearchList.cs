using System;
using System.Threading;
using Abstract;
using EnhancedUI.EnhancedScroller;
using Extensions;
using UIManaging.Pages.Common.SongOption.SongList;
using UnityEngine;

namespace UIManaging.Pages.Common.SongOption.Search
{
    public abstract class MusicSearchList<TModel, TItemView, TListModel> : BaseContextDataView<TListModel>, IEnhancedScrollerDelegate
        where TModel : PlayableItemModel
        where TItemView : PlayableItemBase<TModel>
        where TListModel : ISearchListModel<TModel>
    {
        private const float SCROLL_POSITION_THRESHOLD = 0.05f;
        
        [SerializeField] protected EnhancedScroller _scroller;
        [SerializeField] protected EnhancedScrollerCellView _playableCellView;
        [SerializeField] private EnhancedScrollerCellView _loadingCellView;

        private CancellationTokenSource _cancellationTokenSource;
        private GameObject _loadingCellGameObject;

        private bool FirstPageLoaded => ContextData.FirstPageLoaded;
        private bool LastPageLoaded => ContextData.LastPageLoaded;

        private RectTransform _cachedCellViewTransform;
        private string _lastSearchQuery;

        private RectTransform CellViewRectTransform => _cachedCellViewTransform
            ? _cachedCellViewTransform
            : (_cachedCellViewTransform = _playableCellView.GetComponent<RectTransform>());

        protected override void OnInitialized()
        {
            _scroller.Delegate = this;
            _scroller.scrollerScrolled += ScrollerScrolled;

            ContextData.DataChanged += OnModelChanged;
            ContextData.FetchFailed += OnModelChanged;
            
            _cancellationTokenSource = new CancellationTokenSource();
        }

        protected override void BeforeCleanup()
        {
            _scroller.ClearAll();
            _scroller.Delegate = null;
            _scroller.scrollerScrolled -= ScrollerScrolled;
            
            _cancellationTokenSource?.CancelAndDispose();
            
            if (ContextData == null) return;

            ContextData.DataChanged -= OnModelChanged;
            ContextData.FetchFailed -= OnModelChanged;
        }

        public virtual int GetNumberOfCells(EnhancedScroller scroller)
        { 
            return !FirstPageLoaded || !LastPageLoaded ? ContextData.Models.Count + 1 : ContextData.Models.Count;
        }

        public virtual float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return scroller.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Horizontal
                ? CellViewRectTransform.sizeDelta.x
                : CellViewRectTransform.sizeDelta.y;
        }

        protected virtual void OnModelChanged()
        {
            // reload data if new search is performed
            if (!string.Equals(_lastSearchQuery, ContextData.SearchQuery))
            {
                _lastSearchQuery = ContextData.SearchQuery;
                _scroller.ReloadData();
            }
            else
            {
                _scroller._Resize(true);
            }
            
            if (FirstPageLoaded && _loadingCellGameObject)
            {
                _loadingCellGameObject.SetActive(false);
            }
        }

        public virtual EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            if (!FirstPageLoaded || (!LastPageLoaded && dataIndex == ContextData.Models.Count))
            {
                var loadingCellView = scroller.GetCellView(_loadingCellView);
                var spinner = loadingCellView.transform.GetChild(0);
                spinner.SetActive(FirstPageLoaded || ContextData.AwaitingData);

                _loadingCellGameObject = loadingCellView.gameObject;

                return loadingCellView;
            }

            var cellView = scroller.GetCellView(_playableCellView);
            var playable = cellView.GetComponent<TItemView>();
            var model = ContextData.Models[dataIndex];

            if (!playable.IsInitialized || playable.ContextData.Id != model.Id)
            {
                playable.Initialize(model);
            }

            return cellView;
        }

        private async void ScrollerScrolled(EnhancedScroller scroller, Vector2 val, float scrollPosition)
        {
            if (_scroller.NormalizedScrollPosition > SCROLL_POSITION_THRESHOLD) return;
            
            if (!FirstPageLoaded || LastPageLoaded || ContextData.AwaitingData) return;

            try
            {
                await ContextData.SearchNextPageAsync(_cancellationTokenSource.Token);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}