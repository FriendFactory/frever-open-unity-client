using Abstract;
using EnhancedUI.EnhancedScroller;
using UIManaging.Pages.InboxPage.Interfaces;
using UnityEngine;

namespace UIManaging.Pages.InboxPage.Views
{
    public class ChatListView: BaseContextDataView<IChatListModel>, IEnhancedScrollerDelegate
    {
        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] private EnhancedScrollerCellView _itemDirectPrefab;
        [SerializeField] private EnhancedScrollerCellView _itemGroupPrefab;
        [SerializeField] private GameObject _noTasksObj;
        
        private float _cellSize;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _enhancedScroller.Delegate = this;
            _enhancedScroller.scrollerScrolled += OnScrollerScrolled;
            _cellSize = _enhancedScroller.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Horizontal 
                ? _itemDirectPrefab.GetComponent<RectTransform>().rect.width
                : _itemDirectPrefab.GetComponent<RectTransform>().rect.height;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _enhancedScroller.scrollerScrolled -= OnScrollerScrolled;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var itemModel = ContextData.Items[dataIndex];
            
            var cellView = scroller.GetCellView(itemModel.Users.Count > 2 ? _itemGroupPrefab : _itemDirectPrefab);
            var itemView = cellView.GetComponent<ChatItemViewBase>();
            
            itemView.Initialize(itemModel);
            
            return cellView;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return _cellSize;
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return ContextData?.Items?.Count ?? 0;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            ContextData.ItemsChanged += UpdateItemList;
            _enhancedScroller.ReloadData();
            
            if (_noTasksObj != null)
            {
                _noTasksObj.SetActive(false);
            }
            
            ContextData.RequestPage();
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();

            if (ContextData != null)
            {
                ContextData.ItemsChanged -= UpdateItemList;
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void UpdateItemList()
        {
            _enhancedScroller._Resize(true);
            
            if (_noTasksObj != null)
            {
                _noTasksObj.SetActive(ContextData.Items.Count == 0);
            }
        }
        
        private void OnScrollerScrolled(EnhancedScroller scroller, Vector2 val, float scrollPosition)
        {
            var scrolledToNextPage = _enhancedScroller.ScrollPosition >= _enhancedScroller.ScrollSize - _cellSize;

            if (!scrolledToNextPage)
            {
                return;
            }
            
            ContextData.RequestPage();
        }
    }
}