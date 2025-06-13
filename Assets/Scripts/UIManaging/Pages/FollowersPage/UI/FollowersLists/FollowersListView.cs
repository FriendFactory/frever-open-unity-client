using Abstract;
using EnhancedUI.EnhancedScroller;
using UIManaging.Pages.FollowersPage.UI.FollowersLists;
using UnityEngine;

namespace UIManaging.Pages.FollowersPage.UI
{
    public class FollowersListView : BaseContextDataView<FollowersListModel>, IEnhancedScrollerDelegate
    {
        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] private EnhancedScrollerCellView _followerViewPrefab;

        private void Awake()
        {
            _enhancedScroller.Delegate = this;
        }

        protected override void OnInitialized() 
        {
            _enhancedScroller.ReloadData();
        }
        
        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            if (ContextData == null)
            {
                return 0;
            }
            
            return ContextData.Followers.Length;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return _followerViewPrefab.GetComponent<RectTransform>().rect.height;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(_followerViewPrefab);
            var followerView = cellView.GetComponent<FollowerView>();
            followerView.Initialize(ContextData.Followers[dataIndex]);
            return cellView;
        }
    }
}