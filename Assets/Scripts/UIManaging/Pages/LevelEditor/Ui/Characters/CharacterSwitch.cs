using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.Characters
{
    public sealed class CharacterSwitch : MonoBehaviour, IEndDragHandler, IBeginDragHandler
    {
        [SerializeField] private RectTransform _panelRect;
        [SerializeField] private RectTransform _centerRect;
        [SerializeField] private ExtendedScrollRect _scrollRect;
        [SerializeField] private CanvasGroup _canvasGroup;
        [Space]
        [FormerlySerializedAs("_characterWheelButtons")]
        [SerializeField] private CharacterSwitchButton[] _characterButtons;
        [Space]
        [SerializeField] private bool _isPostRecordEditor;

        private readonly List<RectTransform> _buttonRects = new List<RectTransform>();
        private Coroutine _lerpingCoroutine;
        private int _toggleDistance;
        private int _characterButtonsCount;
        private ILevelManager _levelManager;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action AnyButtonClicked;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private CharacterSwitchButton[] Buttons => _characterButtons;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        private void Construct(ILevelManager levelManager)
        {
            _levelManager = levelManager;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            var buttonDistance = Buttons[1].RectTransform.anchoredPosition.x - Buttons[0].RectTransform.anchoredPosition.x;
            _toggleDistance = (int) Mathf.Abs(buttonDistance);

            foreach (var button in Buttons)
            {
                button.SetupScrollRect(_scrollRect);
                button.SetupOnBeginDragEvent(OnBeginDrag);
                button.SetupOnEndDragEvent(OnEndDrag);
            }

            for (var i = 0; i < Buttons.Length; i++)
            {
                var button = Buttons[i];
                button.Init(i - 1, _isPostRecordEditor);
            }
        }

        private void OnEnable()
        {
            RefreshButtons();

            _levelManager.EditingCharacterSequenceNumberChanged += UpdateTargetButton;
            _levelManager.CharactersPositionsSwapped += RefreshButtons;
            _scrollRect.OnBeginDragEvent += OnBeginDrag;
            _scrollRect.OnEndDragEvent += OnEndDrag;
            SubscribeToButtons();
        }

        private void OnDisable()
        {
            _levelManager.EditingCharacterSequenceNumberChanged -= UpdateTargetButton;
            _levelManager.CharactersPositionsSwapped -= RefreshButtons;
            _scrollRect.OnBeginDragEvent -= OnBeginDrag;
            _scrollRect.OnEndDragEvent -= OnEndDrag;
            UnSubscribeToButtons();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        private void ShowButtonAsSelected(CharacterSwitchButton currentButton)
        {
            foreach (var button in _characterButtons)
            {
                button.IsSelected = button == currentButton;
            }

            SlideToButton(currentButton, true);
        }

        private void SlideToButton(CharacterSwitchButton nextTargetButton, bool shouldSlideInstant)
        {
            if (nextTargetButton != null)
            {
                SlideToPosition(nextTargetButton.TargetSequenceNumber + 1, shouldSlideInstant);
            }
        }

        //---------------------------------------------------------------------
        // IDragHandler
        //---------------------------------------------------------------------

        public void OnBeginDrag(PointerEventData eventData)
        {
            OnBeginDrag();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            OnEndDrag();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void RefreshButtons()
        {
            var charactersCount = _levelManager.TargetEvent.CharacterController.Count;

            var isVisible = charactersCount > 1;
            SetVisible(isVisible);
            if (!isVisible) return;

            _characterButtonsCount = charactersCount + 1;
            _buttonRects.Clear();

            foreach (var button in _characterButtons)
            {
                button.SetActive(false);
            }

            for (var i = 0; i < _characterButtonsCount; i++)
            {
                _characterButtons[i].SetActive(true);
                _characterButtons[i].RefreshThumbnail();
                _buttonRects.Add(_characterButtons[i].RectTransform);
            }

            UpdateTargetButton();
        }

        private void SetVisible(bool isVisible)
        {
            _canvasGroup.alpha = (isVisible) ? 1f : 0f;
            _canvasGroup.blocksRaycasts = isVisible;
            _scrollRect.enabled = isVisible;
        }

        private void UpdateTargetButton()
        {
            var targetButton = _characterButtons.FirstOrDefault(x => x.TargetSequenceNumber == _levelManager.EditingCharacterSequenceNumber);
            ShowButtonAsSelected(targetButton);
        }

        private void OnBeginDrag()
        {
            StopCurrentSlide();
        }

        private void OnEndDrag()
        {
            var indexToClosestButton = GetIndexOfClosestButton();
            _characterButtons[indexToClosestButton].OnClick();

            SlideToPosition(indexToClosestButton, false);
        }

        private IEnumerator LerpToPosition(float targetPos)
        {
            while (Mathf.Abs(_panelRect.anchoredPosition.x - targetPos) > 0.1f)
            {
                var newX = Mathf.Lerp(_panelRect.anchoredPosition.x, targetPos, Time.deltaTime * 20f);
                var newPosition = new Vector2(newX, _panelRect.anchoredPosition.y);
                _panelRect.anchoredPosition = newPosition;
                yield return null;
            }

            _panelRect.anchoredPosition = new Vector2(targetPos, _panelRect.anchoredPosition.y);
            _lerpingCoroutine = null;
            SetButtonsInteractivity(true);
        }

        private void SlideInstant(float targetPos)
        {
            _panelRect.anchoredPosition = new Vector2(targetPos, _panelRect.anchoredPosition.y);
        }

        private void SlideToPosition(int position, bool instant)
        {
            var targetPos = position * -_toggleDistance;
            if (instant)
            {
                SlideInstant(targetPos);
                return;
            }

            StopCurrentSlide();
            SetButtonsInteractivity(false);
            _lerpingCoroutine = StartCoroutine(LerpToPosition(targetPos));
        }

        private void SetButtonsInteractivity(bool value)
        {
            if (_characterButtons == null) return;

            foreach (var characterButton in _characterButtons)
            {
                characterButton.SetInteractable(value);
            }
        }

        private void StopCurrentSlide()
        {
            this.SafeStopCoroutine(_lerpingCoroutine);
        }

        private int GetIndexOfClosestButton()
        {
            var indexToClosestButton = -1;
            var currentMinDistance = float.MaxValue;

            for (var i = 0; i < _characterButtonsCount; i++)
            {
                var distanceToCurrentButton = Mathf.Abs(_centerRect.transform.position.x - _buttonRects[i].transform.position.x);
                if (!(distanceToCurrentButton < currentMinDistance)) continue;

                currentMinDistance = distanceToCurrentButton;
                indexToClosestButton = i;
            }

            return indexToClosestButton;
        }

        private void OnAnyButtonClicked()
        {
            AnyButtonClicked?.Invoke();
        }

        private void SubscribeToButtons()
        {
            foreach (var button in Buttons)
            {
                button.ButtonClicked += OnAnyButtonClicked;
            }
        }
        
        private void UnSubscribeToButtons()
        {
            foreach (var button in Buttons)
            {
                button.ButtonClicked -= OnAnyButtonClicked;
            }
        }
    }
}