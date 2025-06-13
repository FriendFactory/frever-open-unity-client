using UIManaging.EnhancedScrollerComponents.CellSpawners;

namespace UIManaging.Pages.Common.SongOption.SongList
{
    internal sealed class GenrePanelGridSpawner : EnhancedScrollerGridSpawner<SongItem, SongListRow, PlayableSongModel>
    {
        public void SetRowSize(float size)
        {
            RowSize = size;
        }

        public float GetRowSize()
        {
            return RowSize;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            ReloadData();
        }
    }
}