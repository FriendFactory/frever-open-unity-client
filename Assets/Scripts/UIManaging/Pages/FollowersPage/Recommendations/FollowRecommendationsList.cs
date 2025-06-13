using Abstract;
using EnhancedUI.EnhancedScroller;
using Modules.FollowRecommendations;
using UnityEngine;

namespace UIManaging.Pages.FollowersPage.Recommendations
{
    internal class FollowRecommendationsList : BaseContextDataView<IFollowRecommendationsListModel>, IEnhancedScrollerDelegate
    {
        [SerializeField] private EnhancedScroller _scroller;
        [SerializeField] protected EnhancedScrollerCellView _followRecommendationView;
        
        private float _cellViewSize;
        
        //---------------------------------------------------------------------
        // Messages 
        //---------------------------------------------------------------------

        protected void Awake()
        {
            _cellViewSize = _followRecommendationView.GetComponent<RectTransform>().rect.width;
        }
        
        //---------------------------------------------------------------------
        // IEnhancedScrollerDelegate implementation 
        //---------------------------------------------------------------------

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex) => _cellViewSize;

        public int GetNumberOfCells(EnhancedScroller scroller) => ContextData.Models.Count;

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var viewModel = ContextData.Models[dataIndex];
            var cellView = scroller.GetCellView(_followRecommendationView);
            var recommendationView = cellView.GetComponent<FollowRecommendationItemView>();

            recommendationView.Initialize(viewModel);

            return cellView;
        }
        
        //---------------------------------------------------------------------
        // Protected 
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            _scroller.Delegate = this;
        }

        protected override void BeforeCleanup()
        {
            _scroller.ClearAll();
            _scroller.Delegate = null;
        }
    }
}