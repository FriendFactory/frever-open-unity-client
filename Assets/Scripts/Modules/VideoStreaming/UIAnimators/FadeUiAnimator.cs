using DG.Tweening;
using Extensions;
using System;
using UnityEngine;

namespace UI.UIAnimators
{
    public class FadeUiAnimator : BaseGenericUiAnimator<CanvasGroup>
    {
        public override Sequence PlayHideAnimation(Action callback = null)
        {
            animationTarget.alpha = 1f;

            var sequence = DOTween.Sequence();
            sequence.Append(animationTarget.DOFade(0f, duration).SetEase(easeCurve).Play()).OnComplete(callback.SafeInvoke);
            return sequence;
        }

        public override Sequence PlayShowAnimation(Action callback = null)
        {
            animationTarget.alpha = 0f;
            var sequence = DOTween.Sequence();
            sequence.Append(animationTarget.DOFade(1f, duration).SetEase(easeCurve).Play()).OnComplete(callback.SafeInvoke);
            return sequence;
        }

        public override void PlayHideAnimationInstant()
        {
            animationTarget.alpha = 0f;
        }

        public override void PlayShowAnimationInstant()
        {
            animationTarget.alpha = 1f;
        }
    }
}
