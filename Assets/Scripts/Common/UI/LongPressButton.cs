using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Common.UI
{
    /// <summary>
    /// A standard button that sends an event when clicked.
    /// </summary>
    [AddComponentMenu("UI/LongPressButton", 30)]
    public class LongPressButton : Button, IBeginDragHandler
    {
        public UnityEvent onLongPress;
        
        [SerializeField] private float _longPressThreshold = 0.6f;

        private bool _pointerDown;
        private bool _longPressPassed;
        private float _pointerDownStartTime;
        private float _defaultFadeDuration;
        
        protected override void Awake()
        {
            base.Awake();
            _defaultFadeDuration = colors.fadeDuration;
        }

        private void SetFadeTime(float value)
        {
            var tmpColors = this.colors;
            tmpColors.fadeDuration = value;
            colors = tmpColors;
        }

        private void Update()
        {
            if (!_pointerDown || Time.realtimeSinceStartup - _pointerDownStartTime < _longPressThreshold)
            {
                return;
            }

            _longPressPassed = true;
            OnPointerUp(new PointerEventData(EventSystem.current) {button = PointerEventData.InputButton.Left});
            onLongPress?.Invoke();
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || Input.touchCount > 1) return;

            SetFadeTime(_longPressThreshold);
            _pointerDown = true;
            _pointerDownStartTime = Time.realtimeSinceStartup;

            base.OnPointerDown(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            
            SetFadeTime(_defaultFadeDuration);
            _pointerDown = false;
            _pointerDownStartTime = 0f;
            
            if (!_longPressPassed)
            {
                base.OnPointerUp(eventData);
            }

            _longPressPassed = false;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || Input.touchCount > 1) return;
            base.OnPointerClick(eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            SetFadeTime(_defaultFadeDuration);
            _pointerDown = false;
            _pointerDownStartTime = 0f;
        }
    }
}