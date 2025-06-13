using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UIManaging.Animated.Sequences
{
    [Serializable]
    public sealed class AnimateRotationScale : AnimationSequenceBase
    {
        [Header("Start")]
        [SerializeField] private bool _useCurrentRotation;
        [SerializeField, HideIf("_useCurrentRotation")] private Transform _startRotation;

        [Header("Animation Params")]
        [SerializeField] private float _time = 1.0f;
        [SerializeField] private AnimationCurve _easing = AnimationCurve.Linear(0,0,1,1);

        [Header("Target")]
        [SerializeField] private Transform _targetObject;

        public override void Play(GameObject animatedObject)
        {
            SetStartingRotation(animatedObject);
        }

        private void SetStartingRotation(GameObject animatedObject)
        {
            if (!_useCurrentRotation)
                animatedObject.transform.rotation = _startRotation.rotation;
        }
        
        private Sequence GetSequence(GameObject animatedObject)
        {
            var sequence = DOTween.Sequence();

            sequence.SetEase(_easing)
                    .Append(animatedObject.transform.DOScale(_targetObject.localScale, _time))
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