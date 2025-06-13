using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Extensions;

namespace UI.UIAnimators
{
    public class ColorUiAnimator : BaseGenericUiAnimator<Graphic>
    {
        [SerializeField] private Color _hideColor;
        [SerializeField] private Color _showColor;

        private bool _isBlinkingPlaying;
        
        public override Sequence PlayShowAnimation(Action callback = null)
        {
            var sequence = DOTween.Sequence();
            sequence.Append(animationTarget.DOColor(_showColor, duration).SetEase(easeCurve).Play().OnComplete(callback.SafeInvoke));
            return sequence;
        }

        public override Sequence PlayHideAnimation(Action callback = null)
        {
            var sequence = DOTween.Sequence();
            sequence.Append(animationTarget.DOColor(_hideColor, duration).SetEase(easeCurve).Play().OnComplete(callback.SafeInvoke));
            return sequence;
        }

        public override void PlayShowAnimationInstant()
        {
            animationTarget.color = _showColor;
        }

        public override void PlayHideAnimationInstant()
        {
            animationTarget.color = _hideColor;
        }

        public void PlayBlinkAnimation()
        {
            if (_isBlinkingPlaying) return;
            
            _isBlinkingPlaying = true;
            PlayShowAnimation(() => PlayHideAnimation(() => PlayShowAnimation(() => PlayHideAnimation(()=> _isBlinkingPlaying = false))));
        }
    }
}