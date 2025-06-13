using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIManaging.Common
{
    public class LimitedScrollRect : ExtendedScrollRect
    {
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<PointerEventData> OnDragOverTopBorderEvent;
        public event Action<PointerEventData> OnDragWithDisabledScrollDown;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public bool EnableScrollDown { get; set; } = true;
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void OnDrag(PointerEventData eventData)
        {
            var isScrollingDown = eventData.delta.y > 0;
            if (!EnableScrollDown && isScrollingDown)
            {
                OnDragWithDisabledScrollDown?.Invoke(eventData);
                return;
            }
            
            base.OnDrag(eventData);
            
            var offset = content.anchoredPosition.y;

            if (offset < 0f)
            {
                content.anchoredPosition -= Vector2.up * offset;
                OnDragOverTopBorderEvent?.Invoke(eventData);
            }
        }
    }
}