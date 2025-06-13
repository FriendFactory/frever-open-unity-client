using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using EnhancedUI.EnhancedScroller;

namespace Extensions
{
    public static class EnhancedScrollerExtensions
    {
        public static IEnumerable<EnhancedScrollerCellView> GetActiveViews(this EnhancedScroller enhancedScroller)
        {
            var startIndex = enhancedScroller.StartCellViewIndex;
            var endIndex = enhancedScroller.EndCellViewIndex;

            var hasActiveViews = endIndex != 0 || enhancedScroller.GetCellViewAtDataIndex(0) != null;
            if (!hasActiveViews)
            {
                yield break;
            }
            
            for (var i = startIndex; i <= endIndex; i++)
            {
                yield return enhancedScroller.GetCellViewAtDataIndex(i);
            }
        }
        
        public static TweenerCore<float, float, FloatOptions> DOScroll(this EnhancedScroller target, float endValue,
            float duration)
        {
            var tweenCore = DOTween.To(() => target.NormalizedScrollPosition, x => target.NormalizedScrollPosition = x,
                                       endValue, duration);
            tweenCore.SetTarget(target);
            
            return tweenCore;
        }
    }
}