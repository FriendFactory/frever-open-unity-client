using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Animated.Sequences
{
    [Serializable]
    public class AnimateFadeImageSequence : AnimationSequenceBase
    {
        [SerializeField] private Image _animatedImage;
        
        [Space]
        [SerializeField] private float _startAlpha = 1.0f;
        [Space]
        [SerializeField] private float _time = 1.0f;
        [SerializeField] private AnimationCurve _easing = AnimationCurve.Linear(0,0, 1, 1);
        
        [Space]
        [SerializeField] private float _targetAlpha = 0.0f;

        public void Initialize(float from, float to, float time)
        {
            _startAlpha = from;
            _targetAlpha = to;
            _time = time;
        }

        public override void Play(GameObject animatedObject)
        {
            SetStartingAlpha();

            var sequence = Build();
            sequence.Play();
        }

        private void SetStartingAlpha()
        {
            var animatedImageColor = _animatedImage.color;
            animatedImageColor.a = _startAlpha;
            _animatedImage.color = animatedImageColor;
        }

        public override Sequence Build()
        {
            var sequence = DOTween.Sequence();

            sequence.SetEase(_easing)
                    .Append(_animatedImage.DOFade(_targetAlpha, _time))
                    .AppendCallback(OnSequenceFinished);

            return sequence;
        }

        [Button]
        private void Preview()
        {
            var startAlpha = _animatedImage.color.a;
            AnimationFinished += _ => OnPreviewFinished( startAlpha);
        }

        private void OnPreviewFinished(float startAlpha)
        {
            AnimationFinished -= _ => OnPreviewFinished(startAlpha);
            var animatedImageColor = _animatedImage.color;
            animatedImageColor.a = startAlpha;
            _animatedImage.color = animatedImageColor;
        }
    }
}