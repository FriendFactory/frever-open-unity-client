using System;
using DG.Tweening;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Common.UI
{
    public sealed class TransparencyControl : MonoBehaviour
    {
        [SerializeField] private Graphic[] _targetGraphics;
        [SerializeField] private Ease _ease = Ease.InOutQuad;
        
        private float _currentAlpha;
        private float? _targetAlpha;
        
        private float CurrentAlpha
        {
            get => _currentAlpha;
            set
            {
                SetTransparency(value);
                _currentAlpha = value;
            }
        }
        
        public void Switch(bool isVisible, float duration)
        {
            float endValue = isVisible ? 1 : 0;
            if (_targetAlpha.HasValue && Math.Abs(endValue - _targetAlpha.Value) < 0.001f) return;
           
            _targetAlpha = endValue;
            if (duration > 0.001f)
            {
                gameObject.SetActive(true);
                DOTween.To(() => CurrentAlpha, alpha => CurrentAlpha = alpha, _targetAlpha.Value, duration)
                       .SetEase(_ease)
                       .OnComplete(() => gameObject.SetActive(isVisible));
            }
            else
            {
                CurrentAlpha = _targetAlpha.Value;
                gameObject.SetActive(isVisible);
            }
        }

        private void SetTransparency(float alpha)
        {
            foreach (var graphic in _targetGraphics)
            {
                graphic.SetAlpha(alpha);
            }
        }
    }
}
