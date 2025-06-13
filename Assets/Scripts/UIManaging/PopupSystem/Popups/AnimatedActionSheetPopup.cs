using System;
using System.Threading.Tasks;
using DG.Tweening;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    /// <summary>
    /// Here is ugly attempt to create a unified action sheet popup with reusable animation calculated at runtime.
    /// Synchronous popup manager nature doesn't allow to properly distinguish between different popup states.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class AnimatedActionSheetPopup: ActionSheetPopup
    {
        [Header("Animations")] 
        [SerializeField] private RectTransform _contentRect;
        [SerializeField] private Image _background;
        [Header("Slide")] 
        [SerializeField] private float _slideDuration = 0.5f;
        [SerializeField] private Ease _slideEase = Ease.InOutQuad;
        [Header("Fade")] 
        [SerializeField] private float _fadeDuration = 0.25f;
        [SerializeField] private Ease _fadeEase = Ease.InQuad;
        
        private CanvasGroup _canvasGroup;
        private Vector2 _anchoredPosition;
        private float _targetAlpha;
        
        private float _targetY;
        private float _hiddenY;
        
        private Sequence _slideInSequence;
        private Sequence _slideOutSequence;
        
        private bool IsInitialized { get; set; }

        protected override void Awake()
        {
            base.Awake();
            
            _anchoredPosition = _contentRect.anchoredPosition;
            _targetAlpha = _background.color.a;

            _canvasGroup = GetComponent<CanvasGroup>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            _slideInSequence.Kill();
            _slideOutSequence.Kill();
        }

        public override async void Show()
        {
            base.Show();
            
            if (!IsInitialized)
            {
                // resizing may happen later after GO is shown, so, here is a dirty hack to remove blinking 
                _canvasGroup.alpha = 0f;
                
                await WaitForVerticalResizeAsync();

                _canvasGroup.alpha = 1f;
                
                InitializeSequence();
            }
            
            SlideIn();
        }

        public override void Hide()
        {
            SlideOut(() => base.Hide());
        }

        private void InitializeSequence()
        {
            var size = _contentRect.rect.size;
            
            _targetY = _anchoredPosition.y;
            _hiddenY = _targetY - size.y;
            
            _slideInSequence = DOTween.Sequence()
                       .Append(_contentRect.DOAnchorPosY(_targetY, _slideDuration).SetEase(_slideEase))
                       .Join(_background.DOFade(_targetAlpha, _fadeDuration).SetEase(_fadeEase))
                       .SetAutoKill(false)
                       .Pause();
            
            _slideOutSequence = DOTween.Sequence()
                       .Append(_contentRect.DOAnchorPosY(_hiddenY, _slideDuration).SetEase(_slideEase))
                       .Join(_background.DOFade(0f, _fadeDuration).SetEase(_fadeEase))
                       .SetAutoKill(false)
                       .Pause();
            
            var anchoredPosition = _contentRect.anchoredPosition;
            _contentRect.anchoredPosition = new Vector2(anchoredPosition.x, _hiddenY);
            
            _background.SetAlpha(0f);

            IsInitialized = true;
        }

        private void SlideIn(Action onComplete = null)
        {
            _slideInSequence.Rewind();
            _slideInSequence.Play().OnComplete(() => onComplete?.Invoke());
        }
        
        private void SlideOut(Action onComplete = null)
        {
            _slideOutSequence.Rewind();
            _slideOutSequence.Play().OnComplete(() => onComplete?.Invoke());
        }

        private async Task WaitForVerticalResizeAsync(float timeout = 1f)
        {
            var startTime = Time.time;
            while (_contentRect.rect.size.y <= 0f && Time.time < startTime + timeout)
            {
                await Task.Delay(42);
            }
        }
    }
}