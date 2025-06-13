using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace UIManaging.Animated.Behaviours
{
    public sealed class AnimatedFullscreenOverlayBehaviour : MonoBehaviour
    {
        [SerializeField] private FadeInOutModel _fadeInOutModel;
        [SerializeField] private SlideInOutModel _slideInOutModel;

        private List<Sequence> _sequences = new List<Sequence>();
        private Action _onAnimationCompleted;

        private void OnReset()
        {
            _fadeInOutModel = new FadeInOutModel(alphaIn: 0.87f);
            _slideInOutModel = new SlideInOutModel();
        }

        private void OnDisable()
        {
            Cleanup();
        }
        
        public void PlayInAnimation(Action onAnimationCompleted)
        {
            Cleanup();
            _onAnimationCompleted = onAnimationCompleted;
            var fade = _fadeInOutModel.ToInSequence(OnSequenceCompleted);
            _sequences.Add(fade);
            var slide = _slideInOutModel.ToInSequence(OnSequenceCompleted);
            _sequences.Add(slide);

            fade.Play();
            slide.Play();
        }

        public void PlayOutAnimation(Action onAnimationCompleted)
        {
            Cleanup();
            _onAnimationCompleted = onAnimationCompleted;
            _fadeInOutModel.ToOutSequence(OnSequenceCompleted);
            _slideInOutModel.ToOutSequence(OnSequenceCompleted);
        }

        private void OnSequenceCompleted(Sequence sequence)
        {
            _sequences.Remove(sequence);

            if (_sequences.Count == 0) _onAnimationCompleted?.Invoke();
        }
        
        private void Cleanup()
        {
            _sequences.ForEach(s => s.Kill());
            _sequences.Clear();
        }
    }
}