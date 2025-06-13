using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIManaging.Common
{
    public class SliderEventHandler : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        public Action PointerUp { get; set; }
        public Action PointerDown { get; set; }
    
        public void OnPointerDown(PointerEventData eventData)
        {
            PointerDown?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            PointerUp?.Invoke();
        }
    }
}
