using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UIManaging.Animated.Behaviours
{
    [Serializable]
    public class FadeInOutModel
    {
        public TargetType Target;
        [ShowIf("Target", TargetType.Graphic)] public Graphic Graphic;
        [ShowIf("Target", TargetType.CanvasGroup)] public CanvasGroup CanvasGroup;
        
        [TabGroup("FadeIn")]
        public float DelayIn;
        [TabGroup("FadeIn")]
        public float TimeIn;
        [TabGroup("FadeIn")]
        public float AlphaTargetIn;
        
        [TabGroup("FadeOut")]
        public float DelayOut;
        [TabGroup("FadeOut")]
        public float TimeOut;
        [TabGroup("FadeOut")]
        public float AlphaTargetOut;
        [FormerlySerializedAs("EaseIn")] public Ease Ease = Ease.InOutQuad;

        private Action<Sequence> _onAnimationCompleted;

        public FadeInOutModel(float time = 0.4f, float alphaIn = 1.0f, float alphaOut = 0f)
        {
            TimeOut = time;
            TimeIn = time;
            AlphaTargetIn = alphaIn;
            AlphaTargetOut = alphaOut;
        }

        public void CleanUp()
        {
            _onAnimationCompleted = null;
        }

        public Sequence ToInSequence(Action<Sequence> onAnimationCompleted)
        {
            _onAnimationCompleted = onAnimationCompleted;
            var sequence = DOTween.Sequence();
            
            ApplyDelay(sequence, DelayIn);
            ApplyAnimation(sequence, AlphaTargetIn, TimeIn);
            sequence.AppendCallback(() => OnAnimationCompleted(sequence));

            return sequence;
        }

        public Sequence ToOutSequence(Action<Sequence> onAnimationCompleted)
        {
            _onAnimationCompleted = onAnimationCompleted;
            var sequence = DOTween.Sequence();
            
            ApplyDelay(sequence, DelayIn);
            ApplyAnimation(sequence, AlphaTargetOut, TimeOut);
            sequence.AppendCallback(() => OnAnimationCompleted(sequence));
            
            return sequence;
        }

        private Sequence ApplyDelay(Sequence sequence, float delay)
        {
            if (delay <= 0) return sequence;

            sequence.AppendInterval(TimeIn);
            return sequence;
        }

        private Sequence ApplyAnimation(Sequence sequence, float alphaTarget, float time)
        {
            if (Target == TargetType.Graphic) 
                return sequence.Append(Graphic.DOFade(alphaTarget, time).SetEase(Ease));

            return sequence.Append(CanvasGroup.DOFade(alphaTarget, time).SetEase(Ease));
        }

        private void OnAnimationCompleted(Sequence sequence)
        {
            _onAnimationCompleted?.Invoke(sequence);
            _onAnimationCompleted = null;
        }

        public enum TargetType
        {
            Graphic = 0,
            CanvasGroup = 1,
        }
    }
}