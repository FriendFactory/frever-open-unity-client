using System.Linq;
using EnhancedUI.EnhancedScroller;
using Navigation.Args;
using UIManaging.EnhancedScrollerComponents;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Common
{
    public class VideoGrid : VideoList
    {
        [SerializeField] protected int _itemsInRow = 3;
        [SerializeField] protected float _thumbnailAspectRatio = 0.66f;
        [SerializeField] protected GridLayoutGroup _placeholderGrid;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        public override int GetNumberOfCells(EnhancedScroller scroller)
        {
            if (ContextData?.LevelPreviewArgs == null)
            {
                return 0;
            }

            return Mathf.CeilToInt((float) ContextData.LevelPreviewArgs.Count / _itemsInRow);
        }

        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(_levelPreviewItemsRowPrefab);
            var levelPreview = cellView.GetComponent<IEnhancedScrollerRowItem<BaseLevelItemArgs>>();
            var selectedLevels = ContextData.LevelPreviewArgs.Skip(dataIndex * _itemsInRow).Take(_itemsInRow).ToArray();
            
            levelPreview.Setup(selectedLevels);
            
            return cellView;
        }
        
        protected override void SetCellSize()
        {
            CellSize = _enhancedScroller.GetComponent<RectTransform>().rect.width / _itemsInRow / _thumbnailAspectRatio;
            var spacingOffset = ((float)_itemsInRow - 1) / _itemsInRow * _placeholderGrid.spacing.x;
            _placeholderGrid.cellSize = new Vector2(CellSize * _thumbnailAspectRatio - spacingOffset, CellSize);
        }
    }
}