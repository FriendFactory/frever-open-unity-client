using Abstract;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace UIManaging.Pages.SocialActions
{
    public class SocialActionsList : BaseContextDataView<SocialActionListModel>, IEnhancedScrollerDelegate
    {
        private const float CARD_WIDTH = 870;
        [SerializeField] private EnhancedScroller _scroller;
        [SerializeField] private EnhancedScrollerCellView _cardPrefab;

        private void Awake()
        {
            _scroller.Delegate = this;
        }

        protected override void OnInitialized()
        {
            gameObject.SetActive(true);
            ReloadData();
        }

        public void ReloadData()
        {
            _scroller.ReloadData();
        }

        public void CleanUpCards()
        {
            _scroller.ClearAll();
        }
        
        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return ContextData?.CardModels?.Count ?? 0;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return CARD_WIDTH;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(_cardPrefab);
            
            var model = ContextData.CardModels[dataIndex];
            var cardView = cellView.GetComponent<SocialActionCard>();
            
            if (model == null) return cellView;
            
            // view can be reused, so, we need to perform additional id based check
            if (!cardView.IsInitialized || model.ActionId != cardView.ContextData?.ActionId)
            {
                cardView.Initialize(model);
            }

            return cellView;
        }
    }
}