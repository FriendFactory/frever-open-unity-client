using System;
using System.Threading.Tasks;
using Abstract;
using DG.Tweening;
using EnhancedUI.EnhancedScroller;
using UIManaging.Common.Args.Views.LastLevelsPanelArgs;
using UnityEngine;

namespace UIManaging.Common
{
    public class VideoList : BaseContextDataView<BaseVideoListLoader>, IEnhancedScrollerDelegate
    {
        private const float NEXT_PAGE_SCROLL_POSITION_THRESHOLD = 200f;
        public event Action DataDownloaded;

        [SerializeField] protected EnhancedScroller _enhancedScroller;
        [SerializeField] protected EnhancedScrollerCellView _levelPreviewItemsRowPrefab;
        [SerializeField] protected CanvasGroup _loadingPlaceholder;
        [SerializeField] protected GameObject _noContentPlaceholder;
        
        protected float CellSize;
        
        private bool _awaitingData;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected virtual async void Awake()
        {
            await Task.Yield();
            _enhancedScroller.Delegate = this;
            SetCellSize();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _enhancedScroller.scrollerScrolled -= ScrollerScrolled;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public virtual float GetCellViewSize(EnhancedScroller scroller, int dataIndex) => CellSize;

        public virtual EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(_levelPreviewItemsRowPrefab);
            
            var videoView = cellView.GetComponent<VideoListItem>();
            videoView.Initialize(ContextData.LevelPreviewArgs[dataIndex]);
            
            return cellView;
        }
        
        public virtual int GetNumberOfCells(EnhancedScroller scroller) => ContextData?.LevelPreviewArgs?.Count ?? 0;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            ContextData.NewPageAppended += OnDataDownloadedOnInit;
            ContextData.NewPagePrepended += OnPagePrepended;
            ContextData.LastPageLoaded += OnLastPageDownloaded;

            if (ContextData.LevelPreviewArgs.Count > 0)
            {
                OnDataDownloadedOnInit();
            }
            else
            {
                ContextData.DownloadFirstPage();
            }
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            
            _enhancedScroller.scrollerScrolled -= ScrollerScrolled;
            
            if(ContextData == null) return;
            ContextData.NewPageAppended -= OnDataDownloadedOnInit;
            ContextData.NewPageAppended -= OnPageDownloaded;
            ContextData.NewPagePrepended -= OnPagePrepended;
            ContextData.LastPageLoaded -= OnLastPageDownloaded;
            
            _enhancedScroller._RecycleAllCells();

            if (!_loadingPlaceholder) return;
            _loadingPlaceholder.DOKill();
            _loadingPlaceholder.alpha = 1f;
        }
        
        protected virtual void SetCellSize()
        {
            CellSize = _levelPreviewItemsRowPrefab.GetComponent<RectTransform>().rect.width 
                     / _levelPreviewItemsRowPrefab.GetComponent<RectTransform>().rect.height
                     * _enhancedScroller.GetComponent<RectTransform>().rect.height;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void ScrollerScrolled(EnhancedScroller scroller, Vector2 val, float scrollPosition)
        {
            if (Mathf.Abs( scrollPosition - scroller.ScrollSize) < 1f && _awaitingData)
            {
                scroller.Velocity = Vector2.zero;
                scroller.ScrollPosition = scroller.ScrollSize;
            }
            
            if(ContextData == null || IsDestroyed) return;
            
            var scrolledToNextPage = _enhancedScroller.ScrollSize - _enhancedScroller.ScrollPosition <= NEXT_PAGE_SCROLL_POSITION_THRESHOLD;
            if (_awaitingData || !scrolledToNextPage) return;

            _awaitingData = true;
            ContextData.DownloadNextPage();
        }

        private void OnDataDownloadedOnInit()
        {
            if (IsDestroyed) return;
            _awaitingData = false;
            _enhancedScroller.ReloadData();
            DataDownloaded?.Invoke();

            FadeOutPlaceholder();
            
            _enhancedScroller.scrollerScrolled += ScrollerScrolled;

            ContextData.NewPageAppended -= OnDataDownloadedOnInit;
            ContextData.NewPageAppended += OnPageDownloaded;
        }
        
        private void OnPagePrepended()
        {
            _enhancedScroller.ReloadData(1f - _enhancedScroller.NormalizedScrollPosition);
        }
        
        private void OnPageDownloaded()
        {
            if (IsDestroyed) return;
            _awaitingData = false;
            _enhancedScroller._Resize(true);
            _enhancedScroller._RefreshActive();
        }
        
        private void OnLastPageDownloaded()
        {
            _enhancedScroller.scrollerScrolled -= ScrollerScrolled;
            ContextData.LastPageLoaded -= OnLastPageDownloaded;
            OnPageDownloaded();
            
            FadeOutPlaceholder();

            if (_noContentPlaceholder)
            {
                _noContentPlaceholder.SetActive(ContextData.LevelPreviewArgs.Count == 0);
            }
        }

        private void FadeOutPlaceholder()
        {
            if (_loadingPlaceholder)
            {
                _loadingPlaceholder.DOFade(0, 0.25f);
            }
        }
    }
}