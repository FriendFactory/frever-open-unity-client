using System;
using DG.Tweening;
using UnityEngine;
using Extensions;

namespace UI.UIAnimators
{
    public class RotateUiAnimator: BaseGenericUiAnimator<RectTransform>
    {
        [SerializeField] private Vector3 _showRotation;
        [SerializeField] private Vector3 _hideRotation;
        
        public override Sequence PlayShowAnimation(Action callback = null)
        {
            var sequence = DOTween.Sequence();
            sequence.Append(animationTarget.DORotate(_showRotation, duration).SetEase(easeCurve).Play().OnComplete(callback.SafeInvoke));
            return sequence;
        }

        public override Sequence PlayHideAnimation(Action callback = null)
        {
            var sequence = DOTween.Sequence();
            sequence.Append(animationTarget.DORotate(_hideRotation, duration).SetEase(easeCurve).Play().OnComplete(callback.SafeInvoke));
            return sequence;
        }

        public override void PlayShowAnimationInstant()
        {
            animationTarget.transform.rotation = Quaternion.Euler(_showRotation);
        }

        public override void PlayHideAnimationInstant()
        {
            animationTarget.transform.rotation = Quaternion.Euler(_hideRotation);
        }
    }
}