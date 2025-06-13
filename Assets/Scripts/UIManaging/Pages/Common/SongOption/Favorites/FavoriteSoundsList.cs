using UIManaging.EnhancedScrollerComponents;
using UIManaging.Pages.Common.FavoriteSounds;
using UnityEngine;

namespace UIManaging.Pages.Common.SongOption.Favorites
{
    internal sealed class FavoriteSoundsList : BaseEnhancedScrollerView<FavoriteSoundItem, UsedSoundItemModel>
    {
        public void Reload()
        {
            var scrollPosition = _scroller.ScrollPosition;
            var cellViewSize = GetCellViewSize(_scroller, 0);
            var scrollPositionFactor = 1f - Mathf.Clamp01(_scroller.NormalizedScrollPosition + cellViewSize / _scroller.ScrollSize);
                
            _scroller.ReloadData(scrollPositionFactor);

            _scroller.ScrollPosition = scrollPosition;
        }
    }
}