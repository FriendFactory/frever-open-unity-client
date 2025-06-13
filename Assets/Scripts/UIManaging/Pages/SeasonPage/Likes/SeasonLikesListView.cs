using Abstract;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace UIManaging.Pages.SeasonPage.Likes
{
    internal sealed class SeasonLikesListView : BaseContextDataView<SeasonLikesListModel>, IEnhancedScrollerDelegate
    {
        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] private EnhancedScrollerCellView _headerView;
        [SerializeField] private EnhancedScrollerCellView _itemView;
        [SerializeField] private SeasonLikesListViewAnimator _listViewAnimator;
        
        private float _headerCellHeight;
        private float _milestoneCellHeight;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _enhancedScroller.Delegate = this;
            _headerCellHeight = _headerView.GetComponent<RectTransform>().rect.height;
            _milestoneCellHeight = _itemView.GetComponent<RectTransform>().rect.height;
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void ReloadData()
        {
            if (IsDestroyed) return;
            _enhancedScroller.ReloadData();
        }

        public void UpdateScrollPosition()
        {
            _listViewAnimator.ScrollDown();
        }

        //---------------------------------------------------------------------
        // IEnhancedScrollerDelegate
        //---------------------------------------------------------------------

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return ContextData == null ? 0 : ContextData.Items.Length;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            switch (ContextData.Items[dataIndex])
            {
                case SeasonLikesQuestModel model:
                    return _milestoneCellHeight;
                case SeasonLikesHeaderModel model:
                    return _headerCellHeight;
                default:
                    return 0;
            }
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellItem = ContextData.Items[dataIndex];
            switch (ContextData.Items[dataIndex])
            {
                case SeasonLikesQuestModel cellMilestone:
                {
                    var cellView = scroller.GetCellView(_itemView);
                    var itemView = cellView.GetComponent<SeasonLikesQuestView>();
                    itemView.Initialize(cellMilestone);
                    return cellView;
                }
                case SeasonLikesHeaderModel cellHeader:
                {
                    var cellView = scroller.GetCellView(_headerView);
                    var itemView = cellView.GetComponent<SeasonLikesHeaderView>();
                    itemView.Initialize(cellHeader);
                    return cellView;
                }
                default:
                {
                    Debug.LogWarning($"Cell type is not supported: {cellItem?.GetType()}");
                    return null;
                }
            }
        }
        
        //---------------------------------------------------------------------
        // BaseContextDataView
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            ReloadData();
            _listViewAnimator.Initialize();
        }
    }
}