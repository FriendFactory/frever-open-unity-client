using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIManaging.Pages.Common.Helpers
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class PointerDownButton : UIBehaviour, IPointerDownHandler
    {
        public event Action<PointerEventData> OnPointerDown;
        
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) => OnPointerDown?.Invoke(eventData);
    }
}