using DG.Tweening;
using Extensions;
using Modules.LevelManaging.Editing.LevelManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.AssetButtons
{
    internal sealed class AssetButtonsHolder : MonoBehaviour
    {
        private const string BUTTONS_TWEEN_ID = "ABH_Buttons";
        private const string LABELS_TWEEN_ID = "ABH_Lables";
        private const float COLLAPSE_DURATION = 0.4f;
        private const float LABELS_TIMEOUT = 3f;
        private const float LABELS_FADE_DURATION = 0.3f;

        [SerializeField] private Button _collapseButton;
        [SerializeField] private Image _collapseIcon;
        [Space]
        [SerializeField] private int _staticItemsCount;
        [SerializeField] private int _itemHeight;
        [SerializeField] private int _firstItemOffset;
        [Space]
        [SerializeField] private CanvasGroup _cameraResetCanvas;
        [SerializeField] private CameraResetButton _cameraResetButton;
        [Space]
        [SerializeField] private CanvasGroup[] _collapsibleButtons;
        [Header("Labels")]
        [SerializeField] private TMP_Text[] _staticLabels;
        [SerializeField] private TMP_Text[] _collapsibleLabels;
        [Space]
        [SerializeField] private TextVisibilityButton _textVisibilityButton;
        [Header("Shuffle")]
        [SerializeField] private RectTransform _addonButtonsGroup;
        [SerializeField] private float _addonButtonsOffset = 33f;

        [Inject] private ILevelManager _levelManager;

        private int _collapsibleButtonsCount;
        private int _collapsedHeight;
        private int _expandedHeight;
        private bool _isCollapsing = false;
        private bool _isCollapsed = true;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _collapsibleButtonsCount = _collapsibleButtons.Length;

            _collapsedHeight = _staticItemsCount * _itemHeight + _firstItemOffset;
            _expandedHeight = _collapsedHeight + _collapsibleButtonsCount * _itemHeight;

            EnableCollapsibleButtons(false);
            SetAlphaForCollapsibleButtons(0f);

            _collapseButton.onClick.AddListener(OnCollapseButtonClicked);
            _cameraResetButton.StateChanged += OnCameraResetButtonStateChanged;
        }

        private void OnEnable()
        {
            _levelManager.EventStarted += OnEditorLoaded;
        }

        private void OnDisable()
        {
            _levelManager.EventStarted -= OnEditorLoaded;
        }

        private void OnDestroy()
        {
            _collapseButton.onClick.RemoveListener(OnCollapseButtonClicked);
            _cameraResetButton.StateChanged += OnCameraResetButtonStateChanged;
            DOTween.Kill(BUTTONS_TWEEN_ID);
            DOTween.Kill(LABELS_TWEEN_ID);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnEditorLoaded()
        {
            _levelManager.EventStarted -= OnEditorLoaded;
            FadeLabels(0, LABELS_TIMEOUT, LABELS_FADE_DURATION);
        }

        private void OnCollapseButtonClicked()
        {
            if (_isCollapsing) return;

            _isCollapsing = true;
            _isCollapsed = !_isCollapsed;

            var rectTransform = (RectTransform) transform;
            var sizeDelta = GetSizeDelta(rectTransform);
            var buttonsSequence = DOTween.Sequence().SetId(BUTTONS_TWEEN_ID);

            if (_isCollapsed)
            {
                var hideCameraResetButton = !_cameraResetButton.Interactable;
                var itemsCount = (hideCameraResetButton) ? _collapsibleButtonsCount : _collapsibleButtonsCount + 1;
                var duration = COLLAPSE_DURATION / itemsCount;

                for (var i = _collapsibleButtons.Length - 1; i >= 0; i--)
                {
                    var button = _collapsibleButtons[i];
                    buttonsSequence.Append(button.DOFade(0, duration));
                }

                if (hideCameraResetButton)
                {
                    buttonsSequence.Append(_cameraResetCanvas.DOFade(0, duration));
                    sizeDelta.y -= _itemHeight;
                }

                buttonsSequence.Insert(0, _collapseIcon.transform.DORotate(new Vector3(0, 0, 0), COLLAPSE_DURATION));

                FadeLabels(0, LABELS_TIMEOUT, LABELS_FADE_DURATION);
            }
            else
            {
                EnableCollapsibleButtons(true);

                var showCameraResetButton = _cameraResetCanvas.alpha < 1f;
                var itemsCount = (showCameraResetButton) ? _collapsibleButtonsCount : _collapsibleButtonsCount + 1;
                var duration = COLLAPSE_DURATION / itemsCount;

                if (showCameraResetButton)
                {
                    buttonsSequence.Append(_cameraResetCanvas.DOFade(1, duration));
                }

                foreach (var button in _collapsibleButtons)
                {
                    buttonsSequence.Append(button.DOFade(1, duration));
                }

                buttonsSequence.Insert(0, _collapseIcon.transform.DORotate(new Vector3(0, 0, 180f), COLLAPSE_DURATION));

                FadeLabels(1, COLLAPSE_DURATION, LABELS_FADE_DURATION);
            }

            buttonsSequence.Insert(0, rectTransform.DOSizeDelta(sizeDelta, COLLAPSE_DURATION)).SetEase(Ease.InSine);
            buttonsSequence.Insert(0, _addonButtonsGroup.DOAnchorPosY(-sizeDelta.y - _addonButtonsOffset, COLLAPSE_DURATION)).SetEase(Ease.InSine);

            buttonsSequence.OnComplete(OnTweenCompleted);

            void OnTweenCompleted()
            {
                _isCollapsing = false;
                if (_isCollapsed) EnableCollapsibleButtons(false);
            }
        }

        private Vector2 GetSizeDelta(RectTransform rectTransform)
        {
            var rectSize = rectTransform.GetSize();
            rectSize.y = _isCollapsed ? _collapsedHeight : _expandedHeight;
            return rectSize;
        }

        private void EnableCollapsibleButtons(bool value)
        {
            if (_cameraResetButton.Interactable == value || value)
            {
                _cameraResetButton.SetActive(value);
            }

            foreach (var button in _collapsibleButtons)
            {
                button.SetActive(value);
            }
        }

        private void SetAlphaForCollapsibleButtons(float alpha)
        {
            foreach (var button in _collapsibleButtons)
            {
                button.alpha = alpha;
            }
        }

        private void OnCameraResetButtonStateChanged(bool isActive)
        {
            if (!_isCollapsed) return;
            if (isActive) _cameraResetButton.SetActive(true);

            var sequence = DOTween.Sequence().SetId(BUTTONS_TWEEN_ID);
            var rectTransform = (RectTransform) transform;
            var sizeDelta = GetSizeDelta(rectTransform);
            const float duration = COLLAPSE_DURATION / 2f;

            if (isActive)
            {
                sequence.Append(_cameraResetCanvas.DOFade(1, duration));
            }
            else
            {
                sizeDelta.y -= _itemHeight;
                sequence.Append(_cameraResetCanvas.DOFade(0, duration));
            }

            sequence.Insert(0, rectTransform.DOSizeDelta(sizeDelta, duration)).SetEase(Ease.InSine);
            sequence.Insert(0, _addonButtonsGroup.DOAnchorPosY(-sizeDelta.y - _addonButtonsOffset, duration)).SetEase(Ease.InSine);
        }

        private void FadeLabels(float endValue, float delay, float duration)
        {
            DOTween.Kill(LABELS_TWEEN_ID);
            var labelsSequence = DOTween.Sequence().SetId(LABELS_TWEEN_ID);

            var isActive = endValue > 0f;

            foreach (var label in _staticLabels)
            {
                labelsSequence.Insert(delay, label.DOFade(endValue, duration).OnComplete(OnComplete));
                void OnComplete() => label.SetActive(isActive);
            }

            foreach (var label in _collapsibleLabels)
            {
                labelsSequence.Insert(delay, label.DOFade(endValue, duration).OnComplete(OnComplete));
                void OnComplete() => label.SetActive(isActive);
            }

            labelsSequence.OnComplete(() => _textVisibilityButton.ChangeStateSilently(isActive));
        }
    }
}