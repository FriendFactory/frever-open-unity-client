using System;
using Abstract;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIManaging.Common.ScrollSelector
{
    public class ScrollSelectorView : BaseContextDataView<IScrollSelectorModel>, IEnhancedScrollerDelegate
    {
        [SerializeField] private ExtendedScrollRect _extendedScrollRect;
        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] private EnhancedScrollerCellView _enhancedScrollerCellView;

        private float _cellHeight;
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<int> OnScrollerSnappedEvent;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _enhancedScroller.Delegate = this;
            _cellHeight = _enhancedScrollerCellView.GetComponent<RectTransform>().rect.height;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            if (ContextData == null)
            {
                return 0;
            }
            
            return ContextData.Items.Length;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return _cellHeight;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellsView = scroller.GetCellView(_enhancedScrollerCellView);
            var itemView = cellsView.GetComponent<ScrollSelectorItemView>();
            itemView.Initialize(ContextData.Items[dataIndex]);
            return cellsView;
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            _enhancedScroller.scrollerSnapped = OnScrollerSnapped;
            _enhancedScroller.ReloadData();
            _enhancedScroller.JumpToDataIndex(ContextData.InitialDataIndex, 0.5f, 0.5f, true, EnhancedScroller.TweenType.immediate);
            _enhancedScroller._RefreshActive();
            _extendedScrollRect.OnEndDragEvent += OnScrollRectEndDrag;
        }
        
        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            _extendedScrollRect.OnEndDragEvent -= OnScrollRectEndDrag;
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnScrollRectEndDrag(PointerEventData pointerEventData)
        {
            _enhancedScroller.Snap();
        }

        private void OnScrollerSnapped(EnhancedScroller enhancedScroller, int cellIndex, int dataIndex, EnhancedScrollerCellView cellView)
        {
            OnScrollerSnappedEvent?.Invoke(dataIndex);
        }
    }
}