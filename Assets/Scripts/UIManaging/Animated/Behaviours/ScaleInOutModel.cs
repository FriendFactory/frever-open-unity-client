using System;
using DG.Tweening;
using UnityEngine;

namespace UIManaging.Animated.Behaviours
{
    [Serializable]
    internal class ScaleModel
    {
        public RectTransform Target;
        public float Time = 0.2f;
        public Ease Ease;

        public Vector3 StartScale;
        public Vector3 EndScale;

        private Action<Sequence> _onSequenceCompleted;

        public void SetStartScale()
        {
            Target.localScale = StartScale;
        }
        
        public Sequence ToSequence(Action<Sequence> onSequenceCompleted)
        {
            _onSequenceCompleted = onSequenceCompleted;

            var sequence = DOTween.Sequence();

            sequence.Append(Target.DOScale(EndScale, Time).SetEase(Ease));
            sequence.OnComplete(() => _onSequenceCompleted?.Invoke(sequence));

            return sequence;
        }
    }
}