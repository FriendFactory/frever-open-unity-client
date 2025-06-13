using Abstract;
using EnhancedUI.EnhancedScroller;
using UIManaging.Core;
using UnityEngine;

namespace UIManaging.Common.Carousel
{
    public abstract class CarouselViewBase<TListModel, TItem, TItemModel> : BaseContextDataView<TListModel>, IEnhancedScrollerDelegate
        where TListModel : ICarouselListModel<TItemModel> where TItem: ClickableCarouselItem<TItemModel> where TItemModel : CarouselItemModel
    {
        [SerializeField] private EnhancedScroller _scroller;
        [SerializeField] private SwipeGestureBasedSnapping _enhancedScrollerSnapping;
        [SerializeField] private CarouselProgressView _carouselProgressView;
        [SerializeField] protected EnhancedScrollerCellView _carouselElementCellView;
        
        private int _savedCellIndex;
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _scroller.Delegate = this;
        }

        private void OnEnable()
        {
            _enhancedScrollerSnapping.Snapping += OnScrollerSnapped;
        }

        private void OnDisable()
        {
            _enhancedScrollerSnapping.Snapping -= OnScrollerSnapped;
        }
        
        //---------------------------------------------------------------------
        // IEnhancedScrollerDelegate implementation
        //---------------------------------------------------------------------
        
        public int GetNumberOfCells(EnhancedScroller scroller) => ContextData?.Models?.Count ?? 0;

        public abstract float GetCellViewSize(EnhancedScroller scroller, int dataIndex);

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(_carouselElementCellView);
            var element = cellView.GetComponent<TItem>();
            
            var model = ContextData.Models[dataIndex];
            model.OnItemClicked = () => OnItemClicked(dataIndex);
            element.Initialize(model);
            
            return cellView;
        }
        
        public void IncrementSelectedIndex()
        {
            _savedCellIndex = (_savedCellIndex + 1) % ContextData.Models.Count;
            JumpToLastSavedPosition();
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            _carouselProgressView.Initialize(ContextData.Models.Count);
            
            _scroller.ReloadData();
            JumpToLastSavedPosition();
        }
        
        protected virtual void OnScrollerSnapped(int index)
        {
            _savedCellIndex = index;
            _carouselProgressView.SetActiveIndex(index);
        }

        
        //---------------------------------------------------------------------
        // Helpers 
        //---------------------------------------------------------------------
        
        private void OnItemClicked(int index)
        {
            _scroller.JumpToDataIndex(index, _scroller.snapJumpToOffset, _scroller.snapCellCenterOffset, _scroller.snapUseCellSpacing);
            _carouselProgressView.SetActiveIndex(index);
        }
        
        private void JumpToLastSavedPosition()
        {
            _scroller.JumpToDataIndex(_savedCellIndex, _scroller.snapJumpToOffset, _scroller.snapCellCenterOffset, _scroller.snapUseCellSpacing);
            _scroller._RefreshActive();
            OnScrollerSnapped(_savedCellIndex);
        }
    }
}