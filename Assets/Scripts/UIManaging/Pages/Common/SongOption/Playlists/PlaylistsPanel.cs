using Abstract;
using UIManaging.EnhancedScrollerComponents.CellSpawners;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Common.SongOption.Playlists
{
    public class PlaylistsPanel: BaseContextDataView<PlaylistItemsModel>
    {
        [SerializeField] private Button _seeAllButton;
        [SerializeField] private PlaylistItemsGridSpawner _gridSpawner;

        [Inject] private MusicSelectionStateController _stateController;

        private EnhancedScrollerGridSpawnerModel<PlaylistItemModel> _gridSpawnerModel;

        private void OnEnable()
        {
            _seeAllButton.onClick.AddListener(OnSeeAll);
        }

        private void OnDisable()
        {
            _seeAllButton.onClick.RemoveListener(OnSeeAll);
        }

        protected override void OnInitialized()
        {
            _gridSpawnerModel = new EnhancedScrollerGridSpawnerModel<PlaylistItemModel>();
            _gridSpawnerModel.SetItems(ContextData.Playlists);
            
            _gridSpawner.SetRowsScrollable(false);
            _gridSpawner.SetGridScrollable(false);
            _gridSpawner.Initialize(_gridSpawnerModel);
        }

        private void OnSeeAll()
        {
            var playlistsViewModel = new PlaylistsViewModel("Playlists", ContextData.Playlists);
            _stateController.FireAsync(MusicNavigationCommand.OpenPlaylists, playlistsViewModel);
        }
    }
}