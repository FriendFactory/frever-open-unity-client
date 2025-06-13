using UIManaging.EnhancedScrollerComponents.CellSpawners;

namespace UIManaging.Pages.Common.SongOption.Playlists
{
    public class PlaylistItemsGridSpawner : EnhancedScrollerGridSpawner<PlaylistItem, PlaylistItemsRow, PlaylistItemModel>
    {
        protected override void OnInitialized()
        {
            base.OnInitialized();
            ReloadData();
        }
    }
}