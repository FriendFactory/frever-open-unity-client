using System;
using Common;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection
{
    public class AssetSelectorAnimator : MonoBehaviour
    {
        public event Action OnShrinkAnimationStartedEvent;
        public event Action OnShrinkAnimationCompletedEvent;
        public event Action OnExpandAnimationStartedEvent;
        public event Action OnExpandAnimationCompletedEvent;
        
        [SerializeField] private RectTransform _animationTarget;
        [SerializeField] private Transform _expandArrowIcon;
        [SerializeField] private RectTransform _cancelChangesButton;
        [SerializeField] private Image _cancelChangesButtonIcon;
        [SerializeField] private RectTransform _searchTextInput;
        [SerializeField] private LayoutElement _categoriesCancelButtonPlaceholder;
        [SerializeField] private LayoutElement _categoriesExpandButtonPlaceholder;
        [SerializeField] private LayoutElement _searchPanel;
        [SerializeField] private LayoutElement _categoryPanel;
        [SerializeField] private RectTransform _categoriesViewport;
        [SerializeField] private RectTransform _categoriesContainer;
        [SerializeField] private HorizontalLayoutGroup _categoriesLayoutGroup;
        [SerializeField] private CanvasGroup _notFoundView;
        [SerializeField] private TextMeshProUGUI _notFoundText;
        [SerializeField] private CanvasGroup _collapsedScrollerCanvasGroup;
        [SerializeField] private CanvasGroup _expandedScrollerCanvasGroup;
        
        [Header("Animation parameters")]
        [SerializeField] private float _viewExpandAnimDuration = 0.25f;
        [SerializeField] private float _buttonExpandAnimDuration = 0.25f;
        [SerializeField] private float _cancelButtonFadeAnimDuration = 0.1f;
        [SerializeField] private float _cancelButtonFadeAnimDelay = 0.1f;
        [SerializeField] private float _panelShowAnimDuration = 0.25f;
        [SerializeField] private float _notFoundScreenFadeAnimDuration = 0.15f;
        [SerializeField] private float _gridFadeAnimDuration = 0.05f; 
        [SerializeField] private float _cancelButtonWidth = 140;
        [SerializeField] private float _cancelButtonCollapsedWidth = 50;
        [SerializeField] private float _expandedHeight = 1400;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public bool IsExpanded { get; private set; }
        public bool IsExpanding { get; private set; }
        public bool IsShrinking { get; private set; }
        public bool AlwaysInstantAnimations { get; set; }
        public bool SearchPanelEnabled { get; set; }

        private bool IsCancelButtonExpanded { get; set; }
        private bool IsShrinked { get; set; } = true;
        private bool IsCategoryTabExpanded { get; set; } = true;
        private bool IsCategoriesContainerOverFlowing => _categoriesViewport.rect.width < _categoriesContainer.rect.width;
        private ICoroutineSource CoroutineSourceInstance => CoroutineSource.Instance;

        //---------------------------------------------------------------------
        // Fields
        //---------------------------------------------------------------------
        
        private Vector2 _shrinkedSize;
        private Vector2 _expandedSize;
        
        private Sequence _revertButtonStateSequence;
        private Sequence _revertButtonTranslateToCategorySequence;
        private Sequence _categoryTabOnCancelSequence;
        private Sequence _categoryTabOnExpandSequence;
        private Sequence _searchPanelOnCancelSequence;
        private Sequence _searchPanelShowSequence;
        private Sequence _categoryTabHideSequence;
        private Sequence _expandButtonFlipSequence;
        private Sequence _expandScrollerFadeSequence;
        private Sequence _collapseScrollerFadeSequence;
        private Tween _notFoundScreenFadeTween;
        private Coroutine _revertButtonStateRoutine;

        private RectTransform _categoriesLayoutRect;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _categoriesLayoutRect = _categoriesLayoutGroup.GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            SetupAnimations();
        }
        
        private void OnDestroy()
        {
            _revertButtonStateSequence.Kill();
            _revertButtonTranslateToCategorySequence.Kill();
            _categoryTabOnCancelSequence.Kill();
            _categoryTabOnExpandSequence.Kill();
            _searchPanelOnCancelSequence.Kill();
            _searchPanelShowSequence.Kill();
            _categoryTabHideSequence.Kill();
            _expandButtonFlipSequence.Kill();
            _notFoundScreenFadeTween.Kill();
            if(_revertButtonStateRoutine != null) CoroutineSourceInstance.StopCoroutine(_revertButtonStateRoutine);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void SetShrinkedAndExpandedSizes(float shrinkSize)
        {
            _shrinkedSize = new Vector2(_animationTarget.sizeDelta.x, shrinkSize);
            _expandedSize = new Vector2(_shrinkedSize.x, _expandedHeight);
            _animationTarget.sizeDelta = IsShrinked? _shrinkedSize : _expandedSize;
        }
        
        public void PlayExpandAnimation(bool instant = false, Action callback = null)
        {
            SetStatusFlags(true, false, false, false);
            PlayAnimationInternal(_expandedSize, instant, OnExpandAnimationCompleted + callback);
            OnExpandAnimationStartedEvent?.Invoke();
            PlayByState(_expandButtonFlipSequence, true, instant);
        }

        public void PlayShrinkAnimation(bool instant = false, Action callback = null)
        {
            SetStatusFlags(false, false, true, false);
            PlayAnimationInternal(_shrinkedSize, instant, OnShrinkAnimationCompleted + callback);
            PlayByState(_expandButtonFlipSequence, false, instant);
            OnShrinkAnimationStartedEvent?.Invoke();
        }
        
        public void ChangeRevertButtonState(bool state)
        {
            _revertButtonStateRoutine = CoroutineSourceInstance.ExecuteAtEndOfFrame(OnEndOfFrame);
            

            void OnEndOfFrame()
            {
                UpdateCategoryTabPadding();
                PlayRevertButtonState(state);
                _revertButtonStateRoutine = null;
            }
        }

        public void PlayCategoryTabState(bool state)
        {
            IsCategoryTabExpanded = state;
            
            PlayByState(_categoryTabHideSequence, !IsCategoryTabExpanded);
            PlayByState(_revertButtonTranslateToCategorySequence, IsExpanded && IsCategoryTabExpanded);

            if (IsCategoryTabExpanded)
            {
                if (IsCategoriesContainerOverFlowing)
                {
                    PlayByState(_categoryTabOnCancelSequence, IsCancelButtonExpanded, true);
                }
            }
            
            if (IsShrinked || IsShrinking)
            {
                PlaySearchFieldState(!IsCategoryTabExpanded);
            }
            else
            {
                PlayByState(_searchPanelOnCancelSequence, !IsCategoryTabExpanded && IsCancelButtonExpanded);
            }
        }

        public void PlayNotFoundScreenState(bool state, string message, bool instant = false)
        {
            PlayByState(_notFoundScreenFadeTween, state, instant);
            _notFoundText.text = message;
        }

        public void PlayScrollersVisibilityState(bool isExpanded)
        {
            if (isExpanded)
            {
                _expandScrollerFadeSequence.Restart();
            }
            else
            {
                _collapseScrollerFadeSequence.Restart();
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void SetupAnimations()
        {
            InitCategoryTabAnimations();
            InitSearchPanelAnimations();
            InitCancelButtonAnimations();
            InitExpandButtonAnimations();
            InitSearchNotFoundAnimation();
            InitScrollersFadeAnimations();
        }
        
        private void InitScrollersFadeAnimations()
        {
            _expandScrollerFadeSequence = DOTween.Sequence();
            _collapseScrollerFadeSequence = DOTween.Sequence();
            
            _expandScrollerFadeSequence.Join(_expandedScrollerCanvasGroup.DOFade(1f, _gridFadeAnimDuration))
                                       .Join(_collapsedScrollerCanvasGroup.DOFade(0f, _gridFadeAnimDuration))
                                       .PrependInterval(_gridFadeAnimDuration)
                                       .SetAutoKill(false)
                                       .OnComplete(() =>
                                        {
                                            _expandedScrollerCanvasGroup.blocksRaycasts = true;
                                            _collapsedScrollerCanvasGroup.blocksRaycasts = false;
                                        })
                                       .Pause();
            
            _collapseScrollerFadeSequence.Join(_expandedScrollerCanvasGroup.DOFade(0f, _gridFadeAnimDuration))
                                         .Join(_collapsedScrollerCanvasGroup.DOFade(1f, _gridFadeAnimDuration))
                                         .PrependInterval(_gridFadeAnimDuration)
                                         .SetAutoKill(false)
                                         .OnComplete(() =>
                                          {
                                              _expandedScrollerCanvasGroup.blocksRaycasts = false;
                                              _collapsedScrollerCanvasGroup.blocksRaycasts = true;
                                          })
                                         .Pause();
        }
        
        private void InitCategoryTabAnimations()
        {
            _categoryTabOnCancelSequence = DOTween.Sequence();
            _categoryTabOnExpandSequence = DOTween.Sequence();
            _categoryTabHideSequence = DOTween.Sequence();

            _categoriesCancelButtonPlaceholder.preferredWidth = _cancelButtonCollapsedWidth;
            
            var categoryTabOnCancelAnim = _categoriesCancelButtonPlaceholder
               .DOPreferredSize(
                    new Vector2(_cancelButtonWidth, 0),
                    _buttonExpandAnimDuration);

            _categoryTabOnCancelSequence.Join(categoryTabOnCancelAnim)
                 .SetEase(Ease.InOutQuad)
                 .SetAutoKill(false)
                 .Pause();

            var categoryTabOnExpandAnim = _categoriesExpandButtonPlaceholder
                .DOPreferredSize(
                    new Vector2(_cancelButtonCollapsedWidth, 0f),
                    _buttonExpandAnimDuration);

            _categoryTabOnExpandSequence.Join(categoryTabOnExpandAnim)
                .SetEase(Ease.InOutQuad)
                .SetAutoKill(false)
                .Pause();

            var categoryTabHideAnim =
                _categoryPanel.DOPreferredSize(Vector2.zero, _panelShowAnimDuration);

            _categoryTabHideSequence.Join(categoryTabHideAnim)
                .SetEase(Ease.InOutQuad)
                .SetAutoKill(false)
                .Pause();
        }
        
        private void InitSearchPanelAnimations()
        {
            _searchPanelShowSequence = DOTween.Sequence();
            _searchPanelOnCancelSequence = DOTween.Sequence();
            
            var searchPanelShowAnim =
                _searchPanel.DOPreferredSize(new Vector2(0, _cancelButtonWidth), _panelShowAnimDuration);

            _searchPanelShowSequence.Join(searchPanelShowAnim)
                .SetEase(Ease.InOutQuad)
                .OnPlay(() => _searchPanel.gameObject.SetActive(true))
                .OnComplete(() => _searchPanel.gameObject.SetActive(true))
                .OnRewind(() => _searchPanel.gameObject.SetActive(false))
                .SetAutoKill(false)
                .Pause();

            var searchFieldTranslateOnCancelAnim = _searchTextInput
                .DOLocalMoveX(_searchTextInput.localPosition.x + _cancelButtonWidth * 0.5f,
                    _buttonExpandAnimDuration);
            var textSizeDelta = _searchTextInput.sizeDelta;
            var searchFieldShrinkOnCancelAnim = _searchTextInput
                .DOSizeDelta(
                    new Vector2(textSizeDelta.x - _cancelButtonWidth, textSizeDelta.y),
                    _buttonExpandAnimDuration);

            _searchPanelOnCancelSequence.Join(searchFieldTranslateOnCancelAnim)
                .Join(searchFieldShrinkOnCancelAnim)
                .SetEase(Ease.InOutQuad)
                .SetAutoKill(false)
                .Pause();
        }

        private void InitCancelButtonAnimations()
        {
            _revertButtonStateSequence = DOTween.Sequence();
            _revertButtonTranslateToCategorySequence = DOTween.Sequence();
            
            var cancelButtonFadeAnimation = _cancelChangesButtonIcon.DOFade(1, _cancelButtonFadeAnimDuration);

            _revertButtonStateSequence.Join(cancelButtonFadeAnimation)
                .PrependInterval(_cancelButtonFadeAnimDelay)
                .SetAutoKill(false)
                .Pause();

            var cancelButtonTranslateToCategoryAnim = _cancelChangesButton.DOAnchorPosY(
                _cancelChangesButton.anchoredPosition.y - _cancelButtonWidth, _panelShowAnimDuration);

            _revertButtonTranslateToCategorySequence.Join(cancelButtonTranslateToCategoryAnim)
                .SetEase(Ease.InOutQuad)
                .SetAutoKill(false)
                .Pause();
        }
        
        private void InitExpandButtonAnimations()
        {
            _expandButtonFlipSequence = DOTween.Sequence();
            
            _expandButtonFlipSequence.Join(_expandArrowIcon.transform
                    .DOLocalRotateQuaternion(Quaternion.Euler(0, 0, -180f), _buttonExpandAnimDuration))
                .PrependInterval(_viewExpandAnimDuration - _buttonExpandAnimDuration)
                .AppendInterval(_viewExpandAnimDuration - _buttonExpandAnimDuration)
                .SetEase(Ease.InOutQuad)
                .SetAutoKill(false)
                .Pause();
        }

       private void InitSearchNotFoundAnimation()
        {
            _notFoundScreenFadeTween = _notFoundView.DOFade(1, _notFoundScreenFadeAnimDuration)
                .SetEase(Ease.OutQuad)
                .SetAutoKill(false)
                .Pause();
        }
       
        private void PlaySearchFieldState(bool state)
        {
            PlayByState(_searchPanelShowSequence, state);

            if (state)
            {
                PlayByState(_searchPanelOnCancelSequence, !IsCategoryTabExpanded && IsCancelButtonExpanded, true);
            }
        }
        
        private void PlayRevertButtonState(bool state)
        {
            if(IsCancelButtonExpanded == state) return;
            
            IsCancelButtonExpanded = state;
            _cancelChangesButton.GetComponent<Graphic>().raycastTarget = IsCancelButtonExpanded;
            PlayByState(_revertButtonStateSequence, IsCancelButtonExpanded);
            
            if (IsCategoriesContainerOverFlowing)
            {
                PlayByState(_categoryTabOnCancelSequence, IsCancelButtonExpanded, true);
            }

            if (!IsCategoryTabExpanded)
            {
                PlayByState(_searchPanelOnCancelSequence, IsCancelButtonExpanded);
            }
        }
        
        private void PlayByState(Tween sequence, bool state, bool instant = false)
        {
            instant = instant || AlwaysInstantAnimations;
            
            if (state)
            {
                if (instant) sequence.Complete();
                else sequence.PlayForward();
            }
            else
            {
                if (instant) sequence.Rewind();
                else sequence.PlayBackwards();
            }
        }

        private void PlayAnimationInternal(Vector2 targetSize, bool instant = false, Action callback = null)
        {
            if (instant)
            {
                _animationTarget.sizeDelta = targetSize;
                callback?.Invoke();
            }
            else
            {
                _animationTarget.DOKill();
                _animationTarget.DOSizeDelta(targetSize, _viewExpandAnimDuration)
                    .SetEase(Ease.InOutQuad)
                    .OnComplete(() => callback?.Invoke());
            }

            if (IsCategoriesContainerOverFlowing)
            {
                PlayByState(_categoryTabOnExpandSequence, IsExpanding, instant);
            }

            if (!SearchPanelEnabled || !IsCategoryTabExpanded) return;
            PlaySearchFieldState(IsExpanding);
            PlayByState(_revertButtonTranslateToCategorySequence, IsExpanding, instant);
        }

        private void SetStatusFlags(bool isExpanding, bool isExpanded, bool isShrinking, bool isShrinked)
        {
            IsExpanding = isExpanding;
            IsExpanded = isExpanded;
            IsShrinking = isShrinking;
            IsShrinked = isShrinked;
        }

        private void OnShrinkAnimationCompleted()
        {
            SetStatusFlags(false, false, false, true);
            OnShrinkAnimationCompletedEvent?.Invoke();
        }
    
        private void OnExpandAnimationCompleted()
        {
            SetStatusFlags(false, true, false, false);
            OnExpandAnimationCompletedEvent?.Invoke();
        }

        //We need to adjust layout padding depending if category container is bigger than the viewport to get them centered
        private void UpdateCategoryTabPadding()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_categoriesContainer);
            var padding = _cancelButtonWidth - _cancelButtonCollapsedWidth;
            _categoriesLayoutGroup.padding.left = IsCategoriesContainerOverFlowing ? 0 : (int) padding;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_categoriesLayoutRect);
        }
    }
}