using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIManaging.Pages.CreatePostPage
{
    public class CreateButtonWidgetTipStateTransition : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [SerializeField] private Button[] _buttons;

        public void OnPointerDown(PointerEventData eventData)
        {
            foreach (var button in _buttons)
            {
                button.OnPointerDown(eventData);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            foreach (var button in _buttons)
            {
                button.OnPointerUp(eventData);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            foreach (var button in _buttons)
            {
                button.OnPointerClick(eventData);
            }
        }
    }
}