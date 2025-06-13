using Abstract;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace UIManaging.Pages.Feed.Remix.Collection
{
    internal abstract class BaseCharacterSelectionButtonList<TListModel> : BaseContextDataView<TListModel>, IEnhancedScrollerDelegate
        where TListModel : BaseCharacterSelectionListModel
    {
        [SerializeField] private EnhancedScroller _scroller;
        [SerializeField] protected EnhancedScrollerCellView _characterButton;
        [SerializeField] private GameObject _loadingIcon;

        private float _cellViewSize;
        
        //---------------------------------------------------------------------
        // Messages 
        //---------------------------------------------------------------------

        protected virtual void Awake()
        {
            _cellViewSize = _characterButton.GetComponent<RectTransform>().rect.height;
        }
        
        //---------------------------------------------------------------------
        // IEnhancedScrollerDelegate implementation 
        //---------------------------------------------------------------------

        public virtual float GetCellViewSize(EnhancedScroller scroller, int dataIndex) => _cellViewSize;
        
        public abstract int GetNumberOfCells(EnhancedScroller scroller);
        public abstract EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex);
        
        //---------------------------------------------------------------------
        // Protected 
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            _scroller.Delegate = this;
            _scroller.scrollerScrolled += ScrollerScrolled;

            ContextData.NewPageAppended += NewPageAppended;
            ContextData.LastPageLoaded += LastPageLoaded;
            
            _loadingIcon.SetActive(true);
            
            ContextData.DownloadNextPage();
        }

        protected override void BeforeCleanup()
        {
            _loadingIcon.SetActive(false);
            
            _scroller.ClearAll();
            _scroller.Delegate = null;
            _scroller.scrollerScrolled -= ScrollerScrolled;
            
            if (ContextData == null) return;
            
            ContextData.NewPageAppended -= NewPageAppended;
            ContextData.LastPageLoaded -= LastPageLoaded;
        }

        protected virtual void ScrollerScrolled(EnhancedScroller scroller, Vector2 val, float scrollPosition)
        {
            if (ContextData.AwaitingData || scroller.NormalizedScrollPosition >= 0f) return;

            ContextData.DownloadNextPage();
        }

        protected virtual void LastPageLoaded()
        {
            _loadingIcon.SetActive(false);
            
            _scroller.scrollerScrolled -= ScrollerScrolled;
            
            ContextData.NewPageAppended -= NewPageAppended;
            ContextData.LastPageLoaded -= LastPageLoaded;
        }

        private void NewPageAppended()
        {
            _loadingIcon.SetActive(false);
            
            var scrollPosition = _scroller.ScrollPosition;

            _scroller.ReloadData();

            _scroller.ScrollPosition = scrollPosition;
        }
    }
}