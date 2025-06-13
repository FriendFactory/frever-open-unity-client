using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UIManaging.Animated.Behaviours
{
    public sealed class AnimatedLoadingBarGradientBehaviour : MonoBehaviour
    {
        [SerializeField] private float _time;
        [SerializeField] private RectTransform _gradientParent;

        private Sequence _sequence;
        private float _progress;

        private void OnDisable()
        {
            _sequence.Kill();
        }

        [Button]
        public void Play()
        {
            _progress = 0.0f;

            UpdateGradientPositionAndSize(_progress);
            _sequence = DOTween.Sequence()
                               .Append(DOVirtual.Float(0.0f, 1.0f, _time, UpdateGradientPositionAndSize)
                                                .SetEase(Ease.Linear))
                               .SetEase(Ease.Linear)
                               .SetLoops(-1);
            _sequence.Play();
        }

        private void UpdateGradientPositionAndSize(float progress)
        {
            _progress = progress;

            // There was possibility that despite object being disabled and animation killed this method was called one last time
            // this check is here to stop this behaviour from causing nullref.
            if (_gradientParent == null)
            {
                return;
            }

            // divided by 3 to get width of one element
            // multiplied by 2 since two lengths of an element are our final position
            // not simplified to "/ 1.5f" since it seems easier to understand and explain this way
            var x = -_gradientParent.rect.width / 3 * 2 * _progress;
            var y = _gradientParent.anchoredPosition.y;
            _gradientParent.anchoredPosition = new Vector2(x, y);
        }
    }
}