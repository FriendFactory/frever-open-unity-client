using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

namespace TweenExtensions
{
    public static class TweenExtensions
    {
        public static void PlayByState(this Tween tween, bool state, bool instant = false)
        {
            if (state)
            {
                if (instant) tween.Complete();
                else tween.PlayForward();
            }
            else
            {
                if (instant) tween.Rewind();
                else tween.PlayBackwards();
            }
        }
        
        public static TweenerCore<float, float, FloatOptions> DOPixelsPerUnitMultiplier(this Image target, float endValue, float duration, bool snapping = false)
        {
            TweenerCore<float, float, FloatOptions> t = DOTween.To(() => target.pixelsPerUnitMultiplier, x => target.pixelsPerUnitMultiplier = x, endValue, duration);
            t.SetOptions(snapping).SetTarget(target);
            return t;
        }
    }
}