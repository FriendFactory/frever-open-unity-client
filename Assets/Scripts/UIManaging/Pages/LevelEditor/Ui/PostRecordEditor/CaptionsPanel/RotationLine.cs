using DG.Tweening;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel
{
    internal sealed class RotationLine: MonoBehaviour
    {
        [SerializeField] private float _animationDuration = 0.25f;
        [SerializeField] private Image _lineImage;
        
        private Tween _displayTween;
        private Tween _hideTween;
        private bool _isShown;
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _displayTween = _lineImage.DOFade(1, _animationDuration).SetAutoKill(false).Pause();
            _hideTween = _lineImage.DOFade(0, _animationDuration).SetAutoKill(false).Pause();
            var rectTransform = transform as RectTransform;
            var canvasRect = _lineImage.canvas.GetComponent<RectTransform>().rect;
            var width = Mathf.Sqrt(Mathf.Pow(canvasRect.width, 2) + Mathf.Pow(canvasRect.height, 2)) * 2;//double screen diagonal size - max size for rotation line
            rectTransform.sizeDelta = new Vector2(width, rectTransform.GetHeight());
            
            HideImmediate();
        }

        private void OnDisable()
        {
            HideImmediate();
        }

        private void OnDestroy()
        {
            _displayTween.Kill();
            _hideTween.Kill();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Show()
        {
            if (_isShown) return;
            _hideTween.Pause();
            _displayTween.Restart();
            _isShown = true;
        }

        public void Hide()
        {
            if (!_isShown) return;
            _displayTween.Pause();
            _hideTween.Restart();
            _isShown = false;
        }

        public void HideImmediate()
        {
            _isShown = false;
            _hideTween.Pause();
            _displayTween.Pause();
            _lineImage.SetAlpha(0);
        }
    }
}