using System;
using System.Collections.Generic;
using DG.Tweening;
using UIManaging.Common.InputFields;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.PublishPage
{
    internal sealed class DescriptionFieldAnimator : MonoBehaviour
    {
        private const float ANIM_SPACER_OFFSET = 1500;

        private const float DESCRIPTION_DEFAULT_HEIGHT = 510;
        private const float DESCRIPTION_EXPANDED_HEIGHT = 750;
        private const float DESCRIPTION_EXPANDED_BACKGROUND_WIDTH = 1500;
        private const float TAG_BUTTONS_EXPANDED_POSITION_Y = 165;
    
        private const float PREVIEW_DEFAULT_HEIGHT = 405;

        [SerializeField] private IgnoredDeselectableAreaAdvancedInputField _inputField;
        [SerializeField] private float _animationTime;
        [SerializeField] private List<CanvasGroup> _fadeOutCanvasGroups;
        [SerializeField] private List<CanvasGroup> _appearingElements;
        [SerializeField] private List<AnimatedColorChangeImage> _animatedColorChanges;
        [SerializeField] private RectTransform _descriptionFieldBackground;
        [SerializeField] private RectTransform _tagButtonsPanel;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private GameObject _bottomButtonsPanel;

        [SerializeField] private LayoutElement _animationSpacer;
        [SerializeField] private LayoutElement _descriptionField;
        [SerializeField] private LayoutElement _previewPanel;

        private readonly Dictionary<Image, Color32> _startColors = new Dictionary<Image, Color32>();

        private void Start()
        {
            _inputField.onSelect.AddListener(OnFieldSelected);
        
            foreach (var item in _animatedColorChanges)
            {
                _startColors.Add(item.Image, item.Image.color);
            }
        }
        public void AnimateExtend()
        {
            _scrollRect.enabled = false;
            PlayFadeOut(false);
            PlayAppear(false);
            PlayMove();
            PlayResize();
            PlayChangeColor();
        }

        public void AnimateShrink()
        {
            _scrollRect.enabled = true;
            PlayFadeOut(true);
            PlayAppear(true);
            PlayReturnBack();
            PlayResizeBack();
            PlayBackColor();
        }

        private void OnFieldSelected(string text)
        {
            AnimateExtend();
        }

        private void PlayFadeOut(bool reverse)
        {
            var endValue = reverse ? 1f : 0f;
            foreach (var item in _fadeOutCanvasGroups)
            {
                item.DOFade(endValue, _animationTime).SetEase(Ease.Linear);
                item.interactable = reverse;
                item.blocksRaycasts = reverse;
            }
        }
        private void PlayAppear(bool reverse)
        {
            var endValue = reverse ? 0f : 1f;
            foreach (var item in _appearingElements)
            {
                item.DOFade(endValue, _animationTime).SetEase(Ease.Linear);
                item.interactable = !reverse;
                item.blocksRaycasts = !reverse;
            }
        }

        private void PlayMove ()
        {
            EnableScrollRect(false);
            _bottomButtonsPanel.SetActive(false);
            _animationSpacer.DOPreferredSize(new Vector2(0, ANIM_SPACER_OFFSET), _animationTime);
        }

        private void PlayReturnBack()
        {
            _animationSpacer.DOPreferredSize(Vector2.zero, _animationTime).OnComplete(() =>
            {
                EnableScrollRect(true);
                _bottomButtonsPanel.SetActive(true);
            });
        }

        private void PlayChangeColor()
        {
            foreach (var item in _animatedColorChanges)
            {
                item.Image.DOColor(item.FinalColor, _animationTime).SetEase(Ease.Linear);
            }
        }

        private void PlayBackColor()
        {
            foreach (var item in _animatedColorChanges)
            {
                var startColor = _startColors[item.Image];
                item.Image.DOColor(startColor, _animationTime).SetEase(Ease.Linear);
            }
        }

        private void PlayResize()
        {
            _descriptionField.DOPreferredSize(new Vector2(0f, DESCRIPTION_EXPANDED_HEIGHT), _animationTime);
            _previewPanel.DOPreferredSize(Vector2.zero, _animationTime);
            _descriptionFieldBackground.DOSizeDelta(new Vector2(DESCRIPTION_EXPANDED_BACKGROUND_WIDTH, DESCRIPTION_EXPANDED_BACKGROUND_WIDTH), _animationTime);
            _tagButtonsPanel.DOAnchorPosY(TAG_BUTTONS_EXPANDED_POSITION_Y, _animationTime);
        }

        private void PlayResizeBack()
        {
            _descriptionField.DOPreferredSize(new Vector2(0f, DESCRIPTION_DEFAULT_HEIGHT), _animationTime);
            _previewPanel.DOPreferredSize(new Vector2(0f, PREVIEW_DEFAULT_HEIGHT), _animationTime);
            _descriptionFieldBackground.DOSizeDelta(Vector2.zero, _animationTime);
            _tagButtonsPanel.DOAnchorPosY(0, _animationTime);
        }

        private void EnableScrollRect(bool isEnabled)
        {
            _scrollRect.verticalNormalizedPosition = 1f;
            _scrollRect.enabled = isEnabled;
            _scrollRect.vertical = isEnabled;
        }

        [Serializable]
        private class AnimatedColorChangeImage
        {
            public Image Image;
            public Color32 FinalColor;
        }
    }
}
