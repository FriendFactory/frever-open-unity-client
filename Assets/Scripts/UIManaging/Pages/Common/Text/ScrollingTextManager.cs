using Common.Abstract;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UIManaging.Pages.Common.Text
{
    public sealed class ScrollingTextManager : BaseContextlessPanel 
    {
        [SerializeField] private TextMeshProUGUI Text;
        [SerializeField] private TextMeshProUGUI TextLooped;
        [SerializeField] private RectTransform _textRect;
        [SerializeField] private RectTransform _maskRect;
        [Header("Settings")]
        [SerializeField] private Vector2 _scrollSpeedRange = new Vector2(40, 75);
        [SerializeField] private Ease _ease = Ease.InSine;

        private Sequence _textScrollingAnimSequence;
        private float _minTextWidthForAnimation;

        protected override bool IsReinitializable => true;

        private void Reset() => _textRect.anchoredPosition = Vector2.zero;

        private void OnEnable() => StartScrolling();

        private void OnDisable() => StopScrolling();

        protected override void OnInitialized()
        {
            _minTextWidthForAnimation = _maskRect.rect.width * (1f - _textRect.anchorMin.x);
            SetupScrollingAnimation();
        }

        protected override void BeforeCleanUp()
        {
            _textScrollingAnimSequence.Kill();
            _textScrollingAnimSequence = null;
            _textRect.anchoredPosition = Vector2.zero;
        }

        public void SetupScrollingAnimation() 
        {
            _textScrollingAnimSequence = DOTween.Sequence();
            var rect = _maskRect.rect;
            var finalPositionX = -Text.preferredWidth;
            var scrollSpeed = GetScrollSpeed();
            _textScrollingAnimSequence.Join(_textRect.DOAnchorPos(new Vector2(finalPositionX, _textRect.anchoredPosition.y), Mathf.Abs(finalPositionX / scrollSpeed)).SetEase(_ease))
                .SetDelay(2)
                .SetLoops(-1, LoopType.Restart)
                .SetAutoKill(false)
                .Pause();
        }

        public void StartScrolling()
        {
            if (Text.preferredWidth <= _minTextWidthForAnimation)
            {
                TextLooped.gameObject.SetActive(false);
                return;
            }
            
            TextLooped.gameObject.SetActive(true);
            
            _textScrollingAnimSequence?.Rewind();
            _textScrollingAnimSequence?.PlayForward();
        }

        public void StopScrolling()
        {
            _textScrollingAnimSequence?.Pause();
        }

        private float GetScrollSpeed()
        {
            var minLength = _minTextWidthForAnimation;
            var length = Text.preferredWidth;
            var lengthFactor = Mathf.Clamp01((length - minLength) / minLength);
            var speed = Mathf.Lerp(_scrollSpeedRange.x, _scrollSpeedRange.y, lengthFactor);

            return speed;
        }
    }
}
