using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace UIManaging.Animated.Behaviours
{
    public sealed class AnimatedSlideInOutBehaviour : MonoBehaviour
    {
        [SerializeField] private SlideInOutModel _slideModel;

        private List<Sequence> _sequences = new List<Sequence>();
        private Action _onAnimationCompleted;

        private bool _isInitialized;
        
        private void Init()
        {
            if (_isInitialized) return;
            _slideModel.CalculatePositions();
            _isInitialized = true;
        }
        
        public void SetInPosition()
        {
            Init();
            _slideModel.Target.anchoredPosition = _slideModel.InTargetPosition;
        }

        public void SetOutPosition()
        {
            Init();
            _slideModel.Target.anchoredPosition = _slideModel.OutTargetPosition;
        }

        public void PlayInAnimation(Action onAnimationCompleted)
        {
            Cleanup();
            
            SetOutPosition();
            var slide = _slideModel.ToInSequence(OnSequenceCompleted);
            _sequences.Add(slide);
            slide.Play();
        }

        public void PlayOutAnimation(Action onAnimationCompleted)
        {
            Cleanup();

            SetInPosition();
            var slide = _slideModel.ToOutSequence(OnSequenceCompleted);
            _sequences.Add(slide);
            slide.Play();
        }

        private void Cleanup()
        {
            _sequences.ForEach(s => s.Kill());
            _sequences.Clear();
            _onAnimationCompleted = null;
        }

        private void OnSequenceCompleted(Sequence sequence)
        {
            _sequences.Remove(sequence);
            
            if(_sequences.Count == 0) _onAnimationCompleted?.Invoke();
        }
    }
}