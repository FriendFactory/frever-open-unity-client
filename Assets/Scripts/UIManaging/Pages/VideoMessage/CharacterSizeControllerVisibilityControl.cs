using System.Linq;
using DigitalRubyShared;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel;
using UIManaging.Pages.VideoMessage.CharacterSizeManaging;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.VideoMessage
{
    internal sealed class CharacterSizeControllerVisibilityControl: MonoBehaviour
    {
        [SerializeField] private float _hideAfterPeriod = 2f;
        [SerializeField] private CharacterSizeController _characterSizeController;
        [SerializeField] private float _clickDetectionAreaMultiplier = 1.25f;
        [SerializeField] private CaptionsPanel _captionsPanel;
        [SerializeField] private RectTransform _touchArea;
        [Inject] private FingersScript _fingersScript;

        private bool CanShow => !_captionsPanel.IsShown;
        
        private float _timeCounter;

        public void Init()
        {
            _timeCounter = _hideAfterPeriod;
            _captionsPanel.Opened += _characterSizeController.HideImmediate;
        }

        public void Run(bool showCharacterRect)
        {
            if (showCharacterRect)
            {
                DisplayCharacterRectangle();
            }
            enabled = true;
        }

        public void Stop()
        {
            enabled = false;
        }

        private void Update()
        {
            if (_characterSizeController.IsShown)
            {
                OnSizePanelEnabledUpdate();
            }
            else
            {
                OnSizePanelDisabledUpdate();
            }
        }

        private void OnSizePanelEnabledUpdate()
        {
            if (IsUserTouchingScreen())
            {
                var shouldDelayHiding = IsTouchInAllowedZone() && IsUserTouchNearCharacter();
                if (shouldDelayHiding)
                {
                    _timeCounter = _hideAfterPeriod;
                    return;
                }
            }

            _timeCounter -= Time.deltaTime;
            if (_timeCounter >= 0) return;
            _characterSizeController.Hide();
        }

        private bool IsUserTouchingScreen()
        {
            return _fingersScript.Touches.Any();
        }
        
        private void OnSizePanelDisabledUpdate()
        {
            if (!IsUserTouchingScreen()) return;

            if (IsTouchInAllowedZone() && IsUserTouchNearCharacter() && CanShow)
            {
                _timeCounter = _hideAfterPeriod;
                DisplayCharacterRectangle();
            }
        }

        private bool IsUserTouchNearCharacter()
        {
            var touch = _fingersScript.Touches.First();
            var touchWorldPos = new Vector2(touch.ScreenX, touch.ScreenY);
            var borders = _characterSizeController.BorderRectTransform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                borders,
                touchWorldPos,
                null,
                out var localTouchPos
            );
            
            var characterRect = _characterSizeController.BorderRectTransform.rect;

            var extendArea = characterRect.width / 2f * (_clickDetectionAreaMultiplier - 1f);
            var rect = new Rect(characterRect.min - new Vector2(extendArea, extendArea), new Vector2(characterRect.width + extendArea * 2, characterRect.height + extendArea * 2));
            return rect.Contains(localTouchPos);
        }

        private void DisplayCharacterRectangle()
        {
            _characterSizeController.Show();
        }
        
        private bool IsTouchInAllowedZone()
        {
            var touch = _fingersScript.Touches.First();
            return RectTransformUtility.RectangleContainsScreenPoint(_touchArea, new Vector2(touch.ScreenX, touch.ScreenY));
        }
    }
}