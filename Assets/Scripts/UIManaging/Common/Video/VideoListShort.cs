using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace UIManaging.Common
{
    public class VideoListShort : VideoList
    {
        private const int MAX_VIDEOS_AMOUNT = 8;
        
        [SerializeField] private EnhancedScrollerCellView _viewAllItemPrefab;

        private float _cellWidthViewAll;
        
        protected override void Awake()
        {
            base.Awake();
            
            _cellWidthViewAll = _viewAllItemPrefab.GetComponent<RectTransform>().rect.width;
        }
        
        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            if (dataIndex != MAX_VIDEOS_AMOUNT) return base.GetCellView(scroller, dataIndex, cellIndex);
            
            var viewAllCell = scroller.GetCellView(_viewAllItemPrefab);
            var viewAllItem = viewAllCell.GetComponent<ViewAllItem>();
            viewAllItem.Initialize(ContextData.OnTaskClicked);
                
            return viewAllCell;
        }

        public override float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return dataIndex == MAX_VIDEOS_AMOUNT ? _cellWidthViewAll : base.GetCellViewSize(scroller, dataIndex);
        }

        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            return Mathf.Min(base.GetNumberOfCells(scroller), MAX_VIDEOS_AMOUNT + 1);
        }
    }
}