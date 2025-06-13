using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI
{
    public sealed class QuickClickDetector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float _clickThreshold = 0.2f;
        private float _pointerDownTime;

        public event Action Clicked;

        public void OnPointerDown(PointerEventData eventData)
        {
            _pointerDownTime = Time.time;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            var clickDuration = Time.time - _pointerDownTime;

            if (clickDuration <= _clickThreshold)
            {
                OnQuickClick();
            }
        }

        private void OnQuickClick()
        {
            Clicked?.Invoke();
        }
    }
}