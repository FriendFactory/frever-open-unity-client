using System.Threading;
using UIManaging.EnhancedScrollerComponents.CellSpawners;
using UnityEngine;

namespace UIManaging.Pages.Common.SongOption.SongDiscovery
{
    internal class GenresView: MusicViewBase<GenresViewModel>
    {
        [SerializeField] private GenreButtonsGridSpawner _gridSpawner;

        protected override string Name => "Playlists";
        
        private EnhancedScrollerGridSpawnerModel<GenreButtonModel> _gridSpawnerModel;

        protected override InitializationResult OnInitialize(GenresViewModel model, CancellationToken token)
        {
            _gridSpawnerModel = new EnhancedScrollerGridSpawnerModel<GenreButtonModel>();
            _gridSpawnerModel.SetItems(model.Playlists);
            
            _gridSpawner.SetRowsScrollable(false);
            _gridSpawner.SetGridScrollable(true);
            _gridSpawner.Initialize(_gridSpawnerModel);
            
            return base.OnInitialize(model, token);
        }
        
        protected override void MoveBack()
        {
            _stateSelectionController.Fire(MusicNavigationCommand.CloseGenres);
        }
    }
}