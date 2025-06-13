using System;
using DG.Tweening;
using Extensions;
using UI.UIAnimators;
using UnityEngine;

namespace Modules.VideoStreaming.UIAnimators
{
    public abstract class RectTransformBasedUiAnimator : BaseGenericUiAnimator<RectTransform>
    {
        protected Sequence PlayAnimation(PositionSettings targetSettings, Action callback = null)
        {
            animationTarget.DOKill();

            var sequence = DOTween.Sequence();
            sequence.Join(animationTarget.DOPivot(targetSettings.Pivot, duration).SetEase(easeCurve));
            sequence.Join(animationTarget.DOAnchorMin(targetSettings.AnchorMin, duration).SetEase(easeCurve));
            sequence.Join(animationTarget.DOAnchorMax(targetSettings.AnchorMax, duration).SetEase(easeCurve));
            sequence.Join(animationTarget.DOAnchorPos(targetSettings.AnchoredPosition, duration).SetEase(easeCurve));
            sequence.SetEase(easeCurve).SetUpdate(true).OnComplete(callback.SafeInvoke).Play();
            return sequence;
        }

        protected void PlayAnimationInstant(PositionSettings targetSettings)
        {
            animationTarget.DOKill();
            animationTarget.SetPivotAndAnchors(targetSettings.Pivot, targetSettings.AnchorMin, targetSettings.AnchorMax);
            animationTarget.anchoredPosition = targetSettings.AnchoredPosition;
        }
    }
}
