using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel
{
    internal sealed class DeleteCaptionAreaAnimator: MonoBehaviour
    {
        [SerializeField] private RectTransform _targetRectTransform;
        [SerializeField] private Image _targetImage;
        [SerializeField] private Vector2 _endSize;
        [SerializeField] private Color _endColor;
        [SerializeField] private float _duration;
        [SerializeField] private TMP_Text _messageText;
        
        private Vector2 _originalSize;
        private Color _originalColor;
        private Sequence _expandSequence;
        private Sequence _shrinkSequence;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        private Sequence ExpandSequence => _expandSequence ?? (_expandSequence = DOTween.Sequence()
           .Join(_targetRectTransform.DOSizeDelta(_endSize, _duration).SetAutoKill(false))
           .Join(_targetImage.DOColor(_endColor, _duration).SetAutoKill(false))
           .Join(_messageText.DOFade(0, _duration).SetAutoKill(false))
           .SetAutoKill(false));

        private Sequence ShrinkSequence => _shrinkSequence ?? (_shrinkSequence = DOTween.Sequence()
           .Join(_targetRectTransform.DOSizeDelta(_originalSize, _duration).SetAutoKill(false))
           .Join(_targetImage.DOColor(_originalColor, _duration).SetAutoKill(false))
           .Join(_messageText.DOFade(1, _duration).SetAutoKill(false))
           .SetAutoKill(false));

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _originalSize = _targetRectTransform.sizeDelta;
            _originalColor = _targetImage.color;
        }

        private void OnDisable()
        {
            StopShrinkingAnimation();
            StopExpandingAnimation();
            _expandSequence.Rewind();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Play()
        {
            StopShrinkingAnimation();
            ExpandSequence.Restart();
        }

        public void PlayBackward()
        {
            StopExpandingAnimation();
            ShrinkSequence.Restart();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void StopExpandingAnimation()
        {
            if (_expandSequence != null && _expandSequence.IsPlaying())
            {
                _expandSequence.Pause();
            }
        }

        private void StopShrinkingAnimation()
        {
            if (_shrinkSequence != null && _shrinkSequence.IsPlaying())
            {
                _shrinkSequence.Pause();
            }
        }
    }
}