using System;
using EnhancedUI.EnhancedScroller;
using UIManaging.Common.SearchPanel;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.StartChat
{
    public class ChatSearchListView: SearchListView
    {
        [SerializeField] private EnhancedScrollerCellView _headerCellViewPrefab;

        private RectTransform _headerCellViewRect;
        
        public event Action OnGroupChat;
        
        protected override void Awake()
        {
            base.Awake();

            _headerCellViewRect = _headerCellViewPrefab.GetComponent<RectTransform>();
        }
        
        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            return base.GetNumberOfCells(scroller) + (ContextData != null && ContextData.IsSearchResult ? 0 : 1);
        }

        public override float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return dataIndex == 0 && (ContextData == null || !ContextData.IsSearchResult) 
                ? _headerCellViewRect.rect.height 
                : base.GetCellViewSize(scroller, dataIndex - (ContextData != null && ContextData.IsSearchResult ? 0 : 1));
        }

        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var isHeader = dataIndex == 0 && (ContextData == null || !ContextData.IsSearchResult);
            
            var cellView = isHeader
                ? scroller.GetCellView(_headerCellViewPrefab) 
                : base.GetCellView(scroller, dataIndex - (ContextData != null && ContextData.IsSearchResult ? 0 : 1), cellIndex);

            if (isHeader)
            {
                var headerView = cellView.GetComponent<ChatDirectHeaderView>();
                headerView.GroupChatClicked = () => OnGroupChat?.Invoke();
            }
            
            return cellView;
        }
    }
}