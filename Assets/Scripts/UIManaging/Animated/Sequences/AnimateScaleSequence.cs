using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UIManaging.Animated.Sequences
{
    [Serializable]
    public sealed class AnimateScaleSequence : AnimationSequenceBase
    {
        [Header("Start")] 
        [SerializeField] private bool _useCurrentScale;
        [SerializeField, HideIf("_useCurrentScale")] private Vector3 _startScale = Vector3.one;
        
        [Header("Animation Parameters")] 
        [SerializeField] private float _time = 1.0f;
        [SerializeField] private AnimationCurve _easing = AnimationCurve.Linear(0, 0, 1, 1);

        [Header("End")] 
        [SerializeField] private Vector3 _targetScale = Vector3.one;

        public void Initialize(Vector3 from, Vector3 to, float time)
        {
            _startScale = from;
            _targetScale = to;
            _time = time;
        }
        
        public override void Play(GameObject animatedObject)
        {
            SetStartingScale(animatedObject);

            var sequence = GetSequence(animatedObject);

            sequence.Play();
        }

        private void SetStartingScale(GameObject animatedObject)
        {
            if (!_useCurrentScale)
                animatedObject.transform.localScale = _startScale;
        }

        private Sequence GetSequence(GameObject animatedObject)
        {
            var sequence = DOTween.Sequence();

            sequence.SetEase(_easing)
                    .Append(animatedObject.transform.DOScale(_targetScale, _time))
                    .AppendCallback(OnSequenceFinished);

            return sequence;
        }
        
        
        [Button]
        private void Preview(GameObject animatedObject)
        {
            var startScale = animatedObject.transform.localScale;
            AnimationFinished += _ => OnPreviewFinished(animatedObject, startScale);
            Play(animatedObject);
        }

        private void OnPreviewFinished(GameObject animatedObject, Vector3 startScale)
        {
            AnimationFinished -= _ => OnPreviewFinished(animatedObject, startScale);
            animatedObject.transform.localScale = startScale;
        }
    }
}