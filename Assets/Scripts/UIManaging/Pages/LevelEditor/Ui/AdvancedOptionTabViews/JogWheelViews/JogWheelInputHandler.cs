using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabViews.JogWheelViews
{
    public class JogWheelInputHandler : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        public event Action<Vector2> DragEnded;
        private ScrollRect _scrollRect;

        private ScrollRect ScrollRect
        {
            get
            {
                if (_scrollRect == null)
                {
                    _scrollRect = GetComponent<ScrollRect>();
                }

                return _scrollRect;
            }
        }
        public void OnBeginDrag(PointerEventData eventData) {}

        public void OnEndDrag(PointerEventData eventData)
        {
            DragEnded?.Invoke(_scrollRect.normalizedPosition);
        }

        public void SetScrollRectNormalizedPosition(Vector2 position)
        {
            ScrollRect.normalizedPosition = position;
        }
        public Vector2 GetScrollRectContentSizeDelta()
        {
            return ScrollRect.content.sizeDelta;
        }

        public void AddOnValueChangedListener(UnityAction<Vector2> action)
        {
            ScrollRect.onValueChanged.AddListener(action);
        }

        public void RemoveAllListeners()
        {
            ScrollRect.onValueChanged.RemoveAllListeners();
        }
    }
}
