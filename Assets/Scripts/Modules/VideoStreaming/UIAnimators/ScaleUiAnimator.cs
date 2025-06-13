using DG.Tweening;
using Extensions;
using System;
using UnityEngine;

namespace UI.UIAnimators
{
    public class ScaleUiAnimator : BaseGenericUiAnimator<RectTransform>
    { 
        [SerializeField] private Vector2 startScale = new Vector2(0.2f, 0.2f);
        [SerializeField] private Vector2 endScale = Vector2.one;

        public override Sequence PlayShowAnimation(Action callback = null)
        {
            animationTarget.localScale = startScale;
            
            var sequence = DOTween.Sequence();
            sequence.Append(animationTarget.DOScale(endScale, duration).SetEase(easeCurve)).Play().OnComplete(callback.SafeInvoke);
            return sequence;
        }

        public override Sequence PlayHideAnimation(Action callback = null)
        {
            var sequence = DOTween.Sequence();
            sequence.Append(animationTarget.DOScale(startScale, duration).SetEase(easeCurve)).Play().OnComplete(callback.SafeInvoke);
            return sequence;
        }

        public override void PlayShowAnimationInstant()
        {
            animationTarget.transform.localScale = endScale;
        }

        public override void PlayHideAnimationInstant()
        {
            animationTarget.transform.localScale = startScale;
        }
    }
}

