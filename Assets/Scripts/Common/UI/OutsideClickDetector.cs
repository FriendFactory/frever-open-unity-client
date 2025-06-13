using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI
{
    /// <summary>
    /// Detects clicks outside of UI game object it's attached to
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class OutsideClickDetector : MonoBehaviour
    {
        private RectTransform _rectTransform;

        public event Action OutsideClickDetected;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (!IsTouchBegan()) return;

            var touchPos = GetTouchPosition();
            if (!EventSystem.current.IsPointerOverGameObject(GetPointerId()) || !RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, touchPos, null))
            {
                OutsideClickDetected?.Invoke();
            }
        }

        private bool IsTouchBegan()
        {
            #if UNITY_EDITOR
            return Input.GetMouseButtonDown(0);
            #else
            return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
            #endif
        }

        private Vector2 GetTouchPosition()
        {
            #if UNITY_EDITOR
            return Input.mousePosition;
            #else
            return Input.GetTouch(0).position;
            #endif
        }

        private int GetPointerId()
        {
            #if UNITY_EDITOR
            return PointerInputModule.kMouseLeftId;
            #else
            return Input.GetTouch(0).fingerId;
            #endif
        }
    }
}