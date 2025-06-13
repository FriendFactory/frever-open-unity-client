using EnhancedUI.EnhancedScroller;

namespace UIManaging.Pages.Home
{
    internal sealed class CollectionsListHorizontalView : CollectionsListView
    {
        private const int BIG_TILE_WIDTH = 645;
        private const int SMALL_TILE_WIDTH = 300;

        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            return ContextData?.Collections?.Count ?? 0;
        }

        public override float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return ContextData.Collections[dataIndex].HasLargeMarketingThumbnail ? BIG_TILE_WIDTH : SMALL_TILE_WIDTH;
        }

        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var model = ContextData.Collections[dataIndex];
            var hasBigThumbnail = model.HasLargeMarketingThumbnail;
            var prefab = hasBigThumbnail ? _bigPrefab : _smallPrefab;

            var cell = scroller.GetCellView(prefab);
            var view = cell.GetComponent<CollectionTileView>();
            view.Initialize(ContextData.Collections[dataIndex]);

            return cell;
        }
    }
}