using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIManaging.Pages.LevelEditor.Ui
{
    public sealed class OnPointerClickHandler : MonoBehaviour, IPointerClickHandler
    {
        public event Action Click;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            Click?.Invoke();
        }
    }
}