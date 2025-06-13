using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UIManaging.Animated.Behaviours
{
    [Serializable]
    internal class SlideInOutModel
    {
        public RectTransform Target;
        public MovementDirection SlideInDirection;
        public float Time = 0.4f;
        public Ease Ease = Ease.InOutQuad;

        public Vector3 InTargetPosition;
        public Vector3 OutTargetPosition;

        private Action<Sequence> _onSequenceCompleted;

        public Sequence ToInSequence(Action<Sequence> onSequenceCompleted)
        {
            _onSequenceCompleted = onSequenceCompleted;
            var sequence = DOTween.Sequence();

            sequence.Append(Target.DOAnchorPos(InTargetPosition, Time).SetEase(Ease));
            sequence.OnComplete(() => _onSequenceCompleted?.Invoke(sequence));
            
            return sequence;
        }
        
        public Sequence ToOutSequence(Action<Sequence> onSequenceCompleted)
        {
            _onSequenceCompleted = onSequenceCompleted;
            var sequence = DOTween.Sequence();

            sequence.Append(Target.DOAnchorPos(OutTargetPosition, Time).SetEase(Ease));
            sequence.OnComplete(() => _onSequenceCompleted?.Invoke(sequence));
            
            return sequence;
        }

        [Button]
        public void CalculatePositions()
        {
            if (Target is null)
            {
                Debug.LogError("Assign reference to animation target.");
                return;
            }

            switch (SlideInDirection)
            {
                case MovementDirection.FromBottom:
                    InTargetPosition = Target.anchoredPosition;
                    OutTargetPosition = InTargetPosition + Vector3.down * Target.rect.height;
                    return;
                case MovementDirection.FromTop:
                    InTargetPosition = Target.anchoredPosition;
                    OutTargetPosition = InTargetPosition + Vector3.up * Target.rect.height;
                    return;
                case MovementDirection.FromLeft:
                    InTargetPosition = Target.anchoredPosition;
                    OutTargetPosition = InTargetPosition + Vector3.left * Target.rect.width;
                    return;
                case MovementDirection.FromRight:
                    InTargetPosition = Target.anchoredPosition;
                    OutTargetPosition = InTargetPosition + Vector3.right * Target.rect.width;
                    return;
            }
        }


        internal enum MovementDirection
        {
            FromLeft = 0,
            FromRight = 1,
            FromTop = 2,
            FromBottom = 3,
        }
    }
}