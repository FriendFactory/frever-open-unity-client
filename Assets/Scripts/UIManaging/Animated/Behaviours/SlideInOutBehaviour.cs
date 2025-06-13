using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UIManaging.Animated.Sequences;
using UnityEngine;

namespace UIManaging.Animated.Behaviours
{
    /// <summary>
    /// Replaced with AnimatedSlideInOutBehaviour
    /// </summary>
    public sealed class SlideInOutBehaviour : MonoBehaviour
    {
        private enum SlideInFrom
        {
            Top = 0,
            Bottom = 1,
            Left = 2,
            Right = 3,
        }

        [SerializeField] private GameObject _animationTarget;
        [SerializeField] private RectTransform _animatedRectTransform;
        [SerializeField] private SlideInFrom _slideInFrom;
        [SerializeField] private Ease _easeIn = Ease.OutSine;
        [SerializeField] private Ease _easeOut = Ease.InSine;
        [SerializeField] private float _time = 0.5f;

        [Space]
        [SerializeField] private AnimationSequence _slideIn;
        [SerializeField] private AnimationSequence _slideOut;
        [SerializeField] private Vector3 _inPosition;
        [SerializeField] private Vector3 _outPosition;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Reset()
        {
            _animationTarget = gameObject;
            _animatedRectTransform = gameObject.GetComponent<RectTransform>();
            
            BuildSequence();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        [Button]
        public void SlideIn(Action onComplete = null)
        {
            _slideIn.Play(_animationTarget, onComplete);
        }

        [Button]
        public void SlideOut(Action onComplete = null)
        {
            _slideOut.Play(_animationTarget, onComplete);
        }

        [Button]
        public void Hide()
        {
            _animatedRectTransform.anchoredPosition = _outPosition;
        }

        [Button]
        public void Show()
        {
            _animatedRectTransform.anchoredPosition = _inPosition;
        }

        public void InitSequence(Vector3 inPosition, Vector3 outPosition)
        {
            _outPosition = outPosition;
            _inPosition = inPosition;

            var inModel = new AnimatePositionModel
            {
                Time = _time,
                FromPosition = outPosition,
                ToPosition = _inPosition,
                Ease = _easeIn,
                UseAnchoredPosition = true
            };
            
            var outModel = new AnimatePositionModel
            {
                Time = _time,
                FromPosition = _inPosition,
                ToPosition = _outPosition,
                Ease = _easeOut,
                UseAnchoredPosition = true
            };
            
            _slideIn = SequenceElementHelper.NewAnimation(SequenceType.Sequence)
                                            .AnimatePosition(inModel);
            
            _slideOut = SequenceElementHelper.NewAnimation(SequenceType.Sequence)
                                             .AnimatePosition(outModel);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        [Button("Rebuild sequence")]
        private void BuildSequence()
        {
            _outPosition = CalculateSlideInStartPosition();
            _inPosition = Vector3.zero;
            
            var inModel = new AnimatePositionModel
            {
                Time = _time,
                FromPosition = _outPosition,
                ToPosition = _inPosition,
                Ease = _easeIn,
                UseAnchoredPosition = true
            };
            
            var outModel = new AnimatePositionModel
            {
                Time = _time,
                FromPosition = _inPosition,
                ToPosition = _outPosition,
                Ease = _easeOut,
                UseAnchoredPosition = true
            };
            
            _slideIn = SequenceElementHelper.NewAnimation(SequenceType.Sequence)
                                            .AnimatePosition(inModel);
            _slideOut = SequenceElementHelper.NewAnimation(SequenceType.Sequence)
                                             .AnimatePosition(outModel);
        }

        private Vector3 CalculateSlideInStartPosition()
        {
            var rectTransform = _animationTarget.GetComponent<RectTransform>();
            switch (_slideInFrom)
            {
                case SlideInFrom.Top:
                    return Vector3.up * rectTransform.rect.height;
                case SlideInFrom.Bottom:
                    return Vector3.down * rectTransform.rect.height;
                case SlideInFrom.Left:
                    return Vector3.left * rectTransform.rect.width;
                case SlideInFrom.Right:
                    return Vector3.right * rectTransform.rect.width;
                default:
                    return Vector3.negativeInfinity;
            }
        }
    }
}