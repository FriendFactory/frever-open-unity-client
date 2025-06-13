using System.Linq;
using Abstract;
using UIManaging.EnhancedScrollerComponents.CellSpawners;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Common.SongOption.SongDiscovery
{
    public sealed class DiscoveryPanel : BaseContextDataView<DiscoveryPanelModel>
    {
        [SerializeField] private Button _seeAllButton;
        [SerializeField] private GenreButtonsGridSpawner _gridSpawner;
        [SerializeField] private int _itemsCount = 3;
        
        [Inject] private MusicSelectionStateController _stateController;
        
        private EnhancedScrollerGridSpawnerModel<GenreButtonModel> _gridSpawnerModel;
        
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
            _gridSpawnerModel = new EnhancedScrollerGridSpawnerModel<GenreButtonModel>();
            _gridSpawnerModel.SetItems(ContextData.AllGenres.Take(_itemsCount));
            
            _gridSpawner.SetRowsScrollable(false);
            _gridSpawner.SetGridScrollable(false);
            _gridSpawner.Initialize(_gridSpawnerModel);
        }
        
        private void OnSeeAll()
        {
            var genresViewModel = new GenresViewModel(ContextData.AllGenres);
            _stateController.FireAsync(MusicNavigationCommand.OpenGenres, genresViewModel);
        }
    }
}
