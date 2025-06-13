using Abstract;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace UIManaging.Pages.Home
{
    internal abstract class CollectionsListView : BaseContextDataView<CollectionListModel>, IEnhancedScrollerDelegate
    {
        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] protected EnhancedScrollerCellView _smallPrefab;
        [SerializeField] protected EnhancedScrollerCellView _bigPrefab;

        private void Awake()
        {
            _enhancedScroller.Delegate = this;
        }

        protected override void OnInitialized()
        {
            _enhancedScroller.ReloadData();
        }

        public virtual int GetNumberOfCells(EnhancedScroller scroller)
        {
            throw new System.NotImplementedException();
        }

        public virtual float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            throw new System.NotImplementedException();
        }

        public virtual EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            throw new System.NotImplementedException();
        }
    }
}