using System.Threading;
using UIManaging.EnhancedScrollerComponents.CellSpawners;
using UnityEngine;

namespace UIManaging.Pages.Common.SongOption.Playlists
{
    internal class PlaylistsView: MusicViewBase<PlaylistsViewModel>
    {
        [SerializeField] private PlaylistItemsGridSpawner _gridSpawner;

        protected override string Name => "Playlists";
        
        private EnhancedScrollerGridSpawnerModel<PlaylistItemModel> _gridSpawnerModel;

        protected override InitializationResult OnInitialize(PlaylistsViewModel model, CancellationToken token)
        {
            _gridSpawnerModel = new EnhancedScrollerGridSpawnerModel<PlaylistItemModel>();
            _gridSpawnerModel.SetItems(model.Playlists);
            
            _gridSpawner.SetRowsScrollable(false);
            _gridSpawner.SetGridScrollable(true);
            _gridSpawner.Initialize(_gridSpawnerModel);
            
            return base.OnInitialize(model, token);
        }
        
        protected override void MoveBack()
        {
            _stateSelectionController.Fire(MusicNavigationCommand.ClosePlaylists);
        }
    }
}