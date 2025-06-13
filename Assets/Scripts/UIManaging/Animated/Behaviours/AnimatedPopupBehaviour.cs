using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace UIManaging.Animated.Behaviours
{
    public sealed class AnimatedPopupBehaviour : MonoBehaviour
    {
        [SerializeField] private bool _playOnEnable;
        [Space]
        [SerializeField] private FadeInOutModel _fadeModel;
        [SerializeField] private ScaleModel _scaleModel;

        private List<Sequence> _sequences = new List<Sequence>();
        private Action _onAnimationCompleted;

        private void OnReset()
        {
            _fadeModel = new FadeInOutModel(alphaIn: 0.6f);
            _scaleModel = new ScaleModel
            {
                StartScale = new Vector3(1.1f, 1.1f, 1.1f),
                EndScale = Vector3.one,
                Ease = Ease.InOutQuad,
                Time = 0.2f
            };
        }

        private void OnEnable()
        {
            if (!_playOnEnable) return;
            
            Play(null);
        }

        private void OnDisable()
        {
            Cleanup();
        }

        public void Play(Action onAnimationCompleted)
        {
            Cleanup();
            _onAnimationCompleted = onAnimationCompleted;

            var fade = _fadeModel.ToInSequence(OnSequenceCompleted);
            _sequences.Add(fade);
            _scaleModel.SetStartScale();
            var scale = _scaleModel.ToSequence(OnSequenceCompleted);
            _sequences.Add(scale);

            fade.Play();
            scale.Play();
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