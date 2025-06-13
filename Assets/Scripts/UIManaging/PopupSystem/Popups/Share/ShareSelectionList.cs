using System;
using System.Collections.Generic;
using Common.Abstract;
using EnhancedUI.EnhancedScroller;
using UIManaging.Localization;
using UnityEngine;
using Zenject;

namespace UIManaging.PopupSystem.Popups.Share
{
    public class ShareSelectionList : BaseContextView<IShareSelectionListModel>, IEnhancedScrollerDelegate
    {
        [SerializeField] private EnhancedScroller _scroller;
        [Space]
        [SerializeField] private EnhancedScrollerCellView _categoryItem;
        [SerializeField] private EnhancedScrollerCellView _chatsNextPageButton;
        [SerializeField] private EnhancedScrollerCellView _friendsNextPageButton;
        [SerializeField] private EnhancedScrollerCellView _emptyResultsItem;
        [SerializeField] private EnhancedScrollerCellView _groupChatsItem;
        [SerializeField] private EnhancedScrollerCellView _directChatsItem;
        [SerializeField] private EnhancedScrollerCellView _crewChatsItem;
        [SerializeField] private EnhancedScrollerCellView _friendsItem;
        
        private Dictionary<Type, float> _cellViewSizeMap;

        protected override void Awake()
        {
            base.Awake();
            
            InitializeCellViewSizeMap();
        }

        protected override void OnInitialized()
        {
            _scroller.Delegate = this;
        }

        protected override void BeforeCleanUp()
        {
            _scroller.Delegate = null;
            _scroller.ClearAll();
        }

        public void ReloadData()
        {
            var scrollPosition = _scroller.ScrollPosition;

            _scroller.ReloadData();

            _scroller.ScrollPosition = scrollPosition;
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return ContextData.Items.Count;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            var model = ContextData.Items[dataIndex];
            var key = model.GetType();
            
            if (!_cellViewSizeMap.ContainsKey(key))
            {
                Debug.LogError($"[{GetType().Name}] Failed to get height for {model.GetType()}");
                return 0;
            }

            return _cellViewSizeMap[key];
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var model = ContextData.Items[dataIndex];

            return GetCellView(model);
        }

        private EnhancedScrollerCellView GetCellView(IShareSelectionItemModel model)
        {
            switch (model)
            {
                case ShareSelectionChatsItemModel shareSelectionChatsItemModel:
                    return GetItemCellView(shareSelectionChatsItemModel);
                case ShareSelectionFriendsItemModel shareSelectionFriendsItemModel:
                    return GetItemCellView(shareSelectionFriendsItemModel);
                case ShareSelectionChatsNextPageButtonModel chatsNextPageButtonModel:
                    return GetNextButtonCellView(chatsNextPageButtonModel);
                case ShareSelectionFriendsNextPageButtonModel friendsNextPageButtonModel:
                    return GetNextButtonCellView(friendsNextPageButtonModel);
                case ShareSelectionCategoryModel categoryModel:
                    var categoryCellView = _scroller.GetCellView(_categoryItem);
                    var category = categoryCellView.GetComponent<ShareSelectionCategoryPanel>();
                    category.Initialize(categoryModel);

                    return categoryCellView;
                case ShareSelectionEmptyResultsPanelModel shareSelectionEmptyResultsPanelModel:
                    var emptyResultsCellView = _scroller.GetCellView(_emptyResultsItem);
                    var emptyResultsPanel = emptyResultsCellView.GetComponent<ShareSelectionEmptyResultsPanel>();
                    if (!emptyResultsPanel.IsInitialized)
                    {
                        emptyResultsPanel.Initialize(shareSelectionEmptyResultsPanelModel);
                    }

                    return emptyResultsCellView;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            EnhancedScrollerCellView GetNextButtonCellView(ShareSelectionNextPageButtonModel nextPageButtonModel)
            {
                var isChatsNextButton = nextPageButtonModel is ShareSelectionChatsNextPageButtonModel;
                var cellView = isChatsNextButton
                    ? _scroller.GetCellView(_chatsNextPageButton)
                    : _scroller.GetCellView(_friendsNextPageButton);
                var nextPageButton = cellView.GetComponent<ShareSelectionLoadNextPageButton>();

                if (!nextPageButton.IsInitialized)
                {
                    nextPageButton.Initialize(nextPageButtonModel);
                }

                return cellView;
            }
            
            EnhancedScrollerCellView GetItemCellView(ShareSelectionItemModel shareSelectionItemModel)
            {
                var isChatsItem = shareSelectionItemModel is ShareSelectionChatsItemModel;
                var cellView = isChatsItem 
                    ? GetChatCellView(shareSelectionItemModel)
                    : _scroller.GetCellView(_friendsItem);
                
                if (isChatsItem)
                {
                    var chatsItem = cellView.GetComponent<ShareSelectionChatsItem>();
                    if (!chatsItem.IsInitialized || chatsItem.ContextData.Id != shareSelectionItemModel.Id)
                    {
                        chatsItem.Initialize((ShareSelectionChatsItemModel)shareSelectionItemModel);
                    }
                }
                else
                {
                    var friendsItem = cellView.GetComponent<ShareSelectionFriendsItem>();
                    
                    if (!friendsItem.IsInitialized || friendsItem.ContextData.Id != shareSelectionItemModel.Id)
                    {
                        friendsItem.Initialize((ShareSelectionFriendsItemModel)shareSelectionItemModel);
                    }
                }

                return cellView;

                EnhancedScrollerCellView GetChatCellView(ShareSelectionItemModel chatModel)
                {
                    switch (chatModel)
                    {
                        case ShareSelectionCrewChatsItemModel _:
                            return _scroller.GetCellView(_crewChatsItem);
                        case ShareSelectionDirectChatsItemModel _:
                            return _scroller.GetCellView(_directChatsItem);
                        case ShareSelectionGroupChatsItemModel _:
                            return _scroller.GetCellView(_groupChatsItem);
                        default:
                            throw new ArgumentOutOfRangeException(nameof(shareSelectionItemModel));
                    }
                }
            }
        }

        private void InitializeCellViewSizeMap()
        {
            _cellViewSizeMap = new Dictionary<Type, float>()
            {
                { typeof(ShareSelectionCategoryModel), _categoryItem.GetComponent<RectTransform>().rect.height },
                { typeof(ShareSelectionChatsNextPageButtonModel), _chatsNextPageButton.GetComponent<RectTransform>().rect.height },
                { typeof(ShareSelectionFriendsNextPageButtonModel), _friendsNextPageButton.GetComponent<RectTransform>().rect.height },
                { typeof(ShareSelectionEmptyResultsPanelModel), _emptyResultsItem.GetComponent<RectTransform>().rect.height },
                { typeof(ShareSelectionDirectChatsItemModel), _directChatsItem.GetComponent<RectTransform>().rect.height },
                { typeof(ShareSelectionGroupChatsItemModel), _groupChatsItem.GetComponent<RectTransform>().rect.height },
                { typeof(ShareSelectionCrewChatsItemModel), _crewChatsItem.GetComponent<RectTransform>().rect.height },
                { typeof(ShareSelectionFriendsItemModel), _friendsItem.GetComponent<RectTransform>().rect.height },
            };
        }
    }
}