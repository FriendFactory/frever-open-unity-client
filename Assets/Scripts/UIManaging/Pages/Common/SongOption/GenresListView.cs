using Abstract;
using EnhancedUI.EnhancedScroller;
using Extensions;
using UIManaging.Pages.Common.SongOption.SongDiscovery;
using UIManaging.Pages.Common.SongOption.SongList;
using UnityEngine;

namespace UIManaging.Pages.Common.SongOption
{
    internal class GenresListView : BaseContextDataView<GenreListModel>, IEnhancedScrollerDelegate
    {
        [SerializeField] private EnhancedScrollerCellView _discoveryPanelCellView;
        [SerializeField] private EnhancedScrollerCellView _genrePanelPrefab;
        [SerializeField] private EnhancedScroller _enhancedScroller;

        [SerializeField] private int _discoveryPanelInsertIndex = 1;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            _enhancedScroller.Delegate = this;
        }
        
        protected override void BeforeCleanup()
        {
            _enhancedScroller.ClearAll();
            _enhancedScroller.Delegate = null;
        }

        //---------------------------------------------------------------------
        // IEnhancedScrollerDelegate
        //---------------------------------------------------------------------
        
        public int GetNumberOfCells(EnhancedScroller scroller) => ContextData.GenrePanelsData.Count + 1;

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return dataIndex == _discoveryPanelInsertIndex 
                ? _discoveryPanelCellView.GetComponent<RectTransform>().GetSize().y 
                : _genrePanelPrefab.GetComponent<RectTransform>().GetSize().y;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            if (dataIndex == _discoveryPanelInsertIndex)
            {
                var discoveryCellView = scroller.GetCellView(_discoveryPanelCellView);
                var discoveryPanel = discoveryCellView.GetComponent<DiscoveryPanel>();
                
                discoveryPanel.Initialize(ContextData.DiscoveryPanelModel);
                
                return discoveryCellView;
            }
            
            dataIndex = dataIndex > _discoveryPanelInsertIndex ? dataIndex - 1 : dataIndex;
            
            var model = ContextData.GenrePanelsData[dataIndex];
            var cellView = scroller.GetCellView(_genrePanelPrefab);
            var genrePanel = cellView.GetComponent<GenrePanel>();
            genrePanel.Initialize(model);
            return cellView;
        }
    }
}