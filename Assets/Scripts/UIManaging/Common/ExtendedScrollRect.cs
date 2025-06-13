using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIManaging.Common
{
    public class ExtendedScrollRect : ScrollRect
    {
        public event Action<PointerEventData> OnBeginDragEvent;
        public event Action<PointerEventData> OnDragEvent;
        public event Action<PointerEventData> OnEndDragEvent;

        private bool _isDragging;

        protected override void OnDisable()
        {
            base.OnDisable();
            _isDragging = false;
        }

        protected bool CanBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return false;

            if (!IsActive())
                return false;

            return true;
        }

        protected bool CanDrag(PointerEventData eventData)
        {
            if (!_isDragging)
                return false;

            if (eventData.button != PointerEventData.InputButton.Left)
                return false;

            if (!IsActive())
                return false;
            
            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out localCursor))
                return false;

            return true;
        }

        protected bool CanEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return false;

            return true;
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            
            if(CanBeginDrag(eventData))
            {
                _isDragging = true;
                OnBeginDragEvent?.Invoke(eventData);
            }
        }

        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);
            
            if(CanDrag(eventData))
            {
                OnDragEvent?.Invoke(eventData);
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            
            if (CanEndDrag(eventData))
            {
                _isDragging = false;
                OnEndDragEvent?.Invoke(eventData);
            }
        }
    }
}