using System;
using System.Threading.Tasks;
using Abstract;
using DG.Tweening;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Comments
{
    internal class CommentsListView : BaseContextDataView<CommentListModel>, IEnhancedScrollerDelegate
    {
        private const int REPLIES_VIEW_SIZE = 120;

        public event Action OnFirstPageLoaded;
        
        //---------------------------------------------------------------------
        // Fields
        //---------------------------------------------------------------------
        
        [SerializeField] private GameObject _noCommentsPlaceholder;
        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] private EnhancedScrollerCellView _commentCellview;
        [SerializeField] private EnhancedScrollerCellView _loadRepliesButton;
        
        private bool _awaitingData;
        private bool _isUpdatingReplies;
        private bool _defaultScrollInertia;
        private float _scrollPositionBeforeReply = -1f;
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Start()
        {
            _enhancedScroller.Delegate = this;
            _defaultScrollInertia = _enhancedScroller.ScrollRect.inertia;
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void ScrollToCommentForReply(long replyToCommentId)
        {
            _scrollPositionBeforeReply = _enhancedScroller.ScrollPosition;
            var jumpToIndex = ContextData.FindIndex(replyToCommentId);
            var position = _enhancedScroller.GetScrollPositionForDataIndex(jumpToIndex, EnhancedScroller.CellViewPositionEnum.Before);
            var scrollDelta = new Vector3(0, position - _scrollPositionBeforeReply);
            _enhancedScroller.Velocity = Vector2.zero;
            _enhancedScroller.ScrollRect.inertia = false;
            _enhancedScroller.Container.DOBlendableLocalMoveBy(scrollDelta, 0.2f)
                                        .SetEase(Ease.OutQuad)
                                        .OnComplete(() => _enhancedScroller.ScrollRect.inertia = _defaultScrollInertia);
        }
        
        public void ReturnToPositionFromReply()
        {
            if (_scrollPositionBeforeReply < 0) return;
            _enhancedScroller.JumpToPosition(_scrollPositionBeforeReply, EnhancedScroller.TweenType.easeInQuad, 0.3f);
            _scrollPositionBeforeReply = -1f;
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return ContextData?.ListItemModels?.Count ?? 0;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            var model = ContextData.ListItemModels[dataIndex];

            switch (model)
            {
                case CommentItemModel commentItemModel:
                {
                    return commentItemModel.ItemHeight;
                }
                case CommentRepliesModel commentRepliesModel:
                {
                    return commentRepliesModel.FullCount > 0 ? REPLIES_VIEW_SIZE : 0;
                }
            }

            return 0;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var model = ContextData.ListItemModels[dataIndex];

            switch (model)
            {
                case CommentItemModel commentItemModel:
                {
                    var cellView = scroller.GetCellView(_commentCellview);
                    var itemView = cellView.GetComponent<CommentItemView>();
                    
                    commentItemModel.FadeInOnLoading = commentItemModel.FadeInOnLoading || !commentItemModel.IsRoot && _isUpdatingReplies && !commentItemModel.HeightAdjusted;
                    itemView.Initialize(commentItemModel);

                    if (commentItemModel.HeightAdjusted) return cellView;
                    
                    commentItemModel.HeightAdjusted = true;
                    commentItemModel.ItemHeight = itemView.Height;
                    _enhancedScroller.ResizeElement(dataIndex, commentItemModel.ItemHeight);

                    return cellView;
            }
                case CommentRepliesModel commentRepliesModel:
                {
                    var cellView = scroller.GetCellView(_loadRepliesButton);
                    var repliesButton =  cellView.GetComponent<LoadRepliesButton>();
                    repliesButton.Initialize(commentRepliesModel);
                    return cellView;
                }
            } 

            return null;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            _enhancedScroller.ReloadData();
            ContextData.LastPageLoaded += LastPageLoaded;
            ContextData.NewPageAppended += FirstPageLoaded;
            ContextData.RepliesLoaded += ReloadListForReplies;
            ContextData.ReplyInserted += OnReplyInserted;
            ContextData.RepliesRemoved += OnRepliesRemoved;
            ContextData.DownloadNextPage();
        }
        
        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            ContextData.NewPageAppended -= FirstPageLoaded;
            ContextData.NewPageAppended -= ReloadList;
            ContextData.NewPageAppended -= ResizeList;
            ContextData.RepliesLoaded -= ReloadListForReplies;
            ContextData.ReplyInserted -= OnReplyInserted;
            ContextData.RepliesRemoved -= OnRepliesRemoved;
            _enhancedScroller.scrollerScrolled -= ScrollerScrolled;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnRepliesRemoved(int startIndex)
        {
            var scrollPosition = _enhancedScroller.ScrollPosition;
            var reversedScrollPosition = _enhancedScroller.ScrollSize - scrollPosition;
            var rootOutOfViewport = _enhancedScroller.StartDataIndex >= startIndex;
            _enhancedScroller.ReloadData();

            if (rootOutOfViewport)
            {
                _enhancedScroller.SetScrollPositionImmediately(_enhancedScroller.ScrollSize - reversedScrollPosition);
            }
            else
            {
                _enhancedScroller.SetScrollPositionImmediately(scrollPosition);
            }
        }

        private void LastPageLoaded()
        {
            _noCommentsPlaceholder.gameObject.SetActive(ContextData.ListItemModels.Count == 0);
            _enhancedScroller.scrollerScrolled -= ScrollerScrolled;
        }

        private void FirstPageLoaded() 
        {
            _noCommentsPlaceholder.gameObject.SetActive(ContextData.ListItemModels.Count == 0);
            _awaitingData = false;
            ContextData.NewPageAppended -= FirstPageLoaded;
            ContextData.NewPageAppended += ResizeList;
            OnFirstPageLoaded?.Invoke();
            _enhancedScroller.ReloadData();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_enhancedScroller.Container); 
            _enhancedScroller.scrollerScrolled += ScrollerScrolled;
        }

        private void ReloadList()   
        {
            _noCommentsPlaceholder.gameObject.SetActive(ContextData.ListItemModels.Count == 0);
            _awaitingData = false;
            var scrollPosition = _enhancedScroller.ScrollPosition;
            _enhancedScroller.ReloadData();
            _enhancedScroller.SetScrollPositionImmediately(scrollPosition);
        }

        private async void ReloadListForReplies()
        {
            _isUpdatingReplies = true;
            ReloadList();
            await Task.Yield();
            await Task.Yield();
            _isUpdatingReplies = false;
        }

        private void OnReplyInserted(int index)
        {
            ReloadListForReplies();
            //TODO: + Pinned count
            index = Math.Max(0, index - 2);
            if (_enhancedScroller.EndDataIndex >= index || _enhancedScroller.StartDataIndex <= index)
            {
                _enhancedScroller.JumpToDataIndex(index, tweenType: EnhancedScroller.TweenType.easeOutQuad, tweenTime: 0.5f);
            }
        }
        
        private void ResizeList()   
        {
            _awaitingData = false;
            _enhancedScroller._Resize(true);
        }

        private void ScrollerScrolled(EnhancedScroller scroller, Vector2 val, float scrollPosition)
        {
            if(ContextData == null || IsDestroyed) return;
            var scrolledToNextPage = ContextData.ListItemModels.Count - _enhancedScroller.EndDataIndex < 5;
            if (_awaitingData || !scrolledToNextPage) return;
            _awaitingData = true;
            ContextData.DownloadNextPage();
        }

    }
}