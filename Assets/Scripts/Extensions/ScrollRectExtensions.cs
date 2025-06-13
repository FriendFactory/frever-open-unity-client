using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine.UI;

namespace Extensions
{
    public static class ScrollRectExtensions
    {
        public static TweenerCore<float, float, FloatOptions> DOVerticalScroll(this ScrollRect target, float endValue,
            float duration)
        {
            var tweenCore = DOTween.To(() => target.verticalNormalizedPosition, x => target.verticalNormalizedPosition = x,
                                       endValue, duration);
            tweenCore.SetTarget(target);
            
            return tweenCore;
        }
    }
}