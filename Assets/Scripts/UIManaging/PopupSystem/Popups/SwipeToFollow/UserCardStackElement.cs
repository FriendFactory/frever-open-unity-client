using System;
using System.Threading.Tasks;
using Common.Abstract;
using DG.Tweening;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups.SwipeToFollow
{
    internal sealed class UserCardStackElement: BaseContextPanel<SwipeToFollowUserCardModel>
    {
        [Header("Animation")]
        [SerializeField] private float _animationDuration = 0.25f;
        [SerializeField] private float _delayBetweenCards = 0.2f;
        [SerializeField] private Ease _ease = Ease.InOutQuad;
        [Header("Fade Effect")]
        [SerializeField] private Graphic _overlay;
        [SerializeField] private float _initialOverlayAlpha = 0.4f;
        [SerializeField] private float _overlayFadeStep = 0.2f;
        
        private RectTransform _rectTransform;

        public RectTransform RectTransform => _rectTransform? _rectTransform : _rectTransform = GetComponent<RectTransform>();
        
        protected override void OnInitialized() { }

        public async Task MoveAsync(Vector2 targetPosition, Vector2 targetScale, int indexFactor)
        {
            var delay = indexFactor * _delayBetweenCards;
            
            RectTransform.DOAnchorPos(targetPosition, _animationDuration)
                .SetEase(_ease)
                .SetDelay(delay);
            RectTransform.DOScale(targetScale, _animationDuration)
                .SetEase(_ease)
                .SetDelay(delay);
            
            await Task.Delay(TimeSpan.FromSeconds(_animationDuration + delay));
            
            if (indexFactor == 0)
            {
                ContextData.Fire(UserCardTrigger.MoveOnTop);
            }
        }

        public void UpdateOverlay(int indexFactor)
        {
            if (indexFactor == 0)
            {
                _overlay.SetAlpha(0f);
                return;
            }
            
            var delay = indexFactor * _delayBetweenCards;
            var targetAlpha = _initialOverlayAlpha + (indexFactor - 1) * _overlayFadeStep;
            
            _overlay.DOFade(targetAlpha, _animationDuration)
                    .SetEase(_ease)
                    .SetDelay(delay);
        }
    }
}