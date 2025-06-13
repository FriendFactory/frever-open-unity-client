using UIManaging.EnhancedScrollerComponents.CellSpawners;
using UIManaging.Pages.Common.SongOption.SongList;

namespace UIManaging.Pages.Common.SongOption.MusicCategory
{
    public class PlaylistPanelGridSpawner :  EnhancedScrollerGridSpawner<TrackItem, TrackListRow, PlayableTrackModel>
    {
        public void SetRowSize(float size)
        {
            RowSize = size;
        }

        public float GetRowSize()
        {
            return RowSize;
        }
    }
}
