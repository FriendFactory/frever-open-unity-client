using System.Linq;
using Abstract;
using EnhancedUI.EnhancedScroller;
using Extensions;
using UIManaging.Pages.Common.SongOption.Playlists;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.SongOption.MusicCategory
{
    internal sealed class PlaylistView : BaseContextDataView<PlaylistViewModel>, IEnhancedScrollerDelegate
    {
        private const int PLAYLISTS_PANEL_INDEX = 1;
        private const int PLAYLISTS_PANEL_ITEMS_COUNT = 6;
        
        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] private EnhancedScrollerCellView _playlistPanelPrefab;
        [SerializeField] private EnhancedScrollerCellView _playlistsPanelCellView;

        [Inject] private MusicDataProvider _musicDataProvider;
        
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

        public int GetNumberOfCells(EnhancedScroller scroller) => ContextData.PlaylistPanelModels.Count + 1;

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return dataIndex == PLAYLISTS_PANEL_INDEX
                ? _playlistsPanelCellView.GetComponent<RectTransform>().GetSize().y
                : _playlistPanelPrefab.GetComponent<RectTransform>().GetSize().y;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            if (dataIndex == PLAYLISTS_PANEL_INDEX)
            {
                var playlistsCellView = scroller.GetCellView(_playlistsPanelCellView);
                var playlistsPanel = playlistsCellView.GetComponent<PlaylistsPanel>();
                var playlistsModel = new PlaylistItemsModel(_musicDataProvider.Playlists.Take(PLAYLISTS_PANEL_ITEMS_COUNT));
                
                playlistsPanel.Initialize(playlistsModel);

                return playlistsCellView;
            }

            dataIndex = dataIndex > PLAYLISTS_PANEL_INDEX ? dataIndex - 1 : dataIndex;
            
            var model = ContextData.PlaylistPanelModels[dataIndex];
            var cellView = scroller.GetCellView(_playlistPanelPrefab);
            var genrePanel = cellView.GetComponent<PlaylistPanel>();
            genrePanel.Initialize(model);
            return cellView;
        }
    }
}