using UIManaging.EnhancedScrollerComponents.CellSpawners;

namespace UIManaging.Pages.Common.SongOption.SongDiscovery
{
    public class GenreButtonsGridSpawner: EnhancedScrollerGridSpawner<GenreButton, GenreButtonsRow, GenreButtonModel>
    {
        protected override void OnInitialized()
        {
            base.OnInitialized();
            ReloadData();
        }
    }
}