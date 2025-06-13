using EnhancedUI.EnhancedScroller;
using Extensions;
using UIManaging.Common;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Tasks.TaskVideosGridPage
{
    internal sealed class TaskVideosGrid : VideoGrid
    {
        [SerializeField] private EnhancedScrollerCellView _headerViewItem;
        [SerializeField] private EnhancedScrollerCellView _cellViewItem;
        
        private float? _cellSize;
        private float _headerCellSize;
        private bool _headerInitalized;

        //---------------------------------------------------------------------
        // Messages 
        //---------------------------------------------------------------------
        
        protected override void Awake()
        {
            _headerViewItem.gameObject.SetActive(false);
            _placeholderGrid.gameObject.SetActive(false);
            base.Awake();
        }
        
        //---------------------------------------------------------------------
        // Protected 
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            InitializeHeaderView();
            base.OnInitialized();
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            _headerCellSize = 0f;
            _headerInitalized = false;
        }

        //---------------------------------------------------------------------
        // IEnhancedScrollerDelegate
        //---------------------------------------------------------------------
        
        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            return base.GetNumberOfCells(scroller) + 1;
        }

        public override float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return dataIndex == 0 
                ? _headerCellSize 
                : base.GetCellViewSize(scroller, dataIndex);
        }
        
        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            if (dataIndex == 0)
            {
                _headerViewItem.gameObject.SetActive(_headerInitalized);
                return _headerViewItem;
            }
            
            //Subtract -1 is index offset for header view at index 0
            return base.GetCellView(scroller, dataIndex - 1, cellIndex - 1);
        }
        
        //---------------------------------------------------------------------
        // Private
        //---------------------------------------------------------------------

        private void InitializeHeaderView()
        {
            _headerViewItem.gameObject.SetActive(true);
            _placeholderGrid.gameObject.SetActive(true);
            
            var headerView = _headerViewItem.GetComponent<TaskVideosGridHeaderView>();
            headerView.Initialize((ContextData as TaskModel)?.CreateViewArgs() ?? default);
            
            var rectTransform = (RectTransform)_headerViewItem.transform;
            _headerCellSize = rectTransform.GetHeight();

            _headerInitalized = true;
            
            var padding = _placeholderGrid.padding;
            padding.top = (int)(_headerCellSize + _enhancedScroller.padding.top + _enhancedScroller.spacing);
            _placeholderGrid.padding = padding;
        }
    }
}