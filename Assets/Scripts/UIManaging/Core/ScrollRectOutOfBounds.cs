using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIManaging.Core
{
    public class ScrollRectOutOfBounds : MonoBehaviour, IEndDragHandler
    {
        public event Action OnGetOutOfBoundsRelease;

        [SerializeField] private float _outOfBoundsOffset = 200f;
        [SerializeField] private bool _updateOnlyDownPull = true;
        [SerializeField] private ScrollRect _scrollRect;


        void Start()
        {
            if(!_scrollRect) _scrollRect = GetComponent<ScrollRect>();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_scrollRect == null) return;

            var rect = _scrollRect.GetComponent<RectTransform>();
            float xMaxPos = _scrollRect.content.rect.width - rect.rect.width;
            float yMaxPos = _scrollRect.content.rect.height - rect.rect.height;

            bool xOutOfBound = _scrollRect.content.anchoredPosition.x < -_outOfBoundsOffset || _scrollRect.content.anchoredPosition.x > xMaxPos + _outOfBoundsOffset;//x < 0 || x > 1;
            bool yOutOfBound = _scrollRect.content.anchoredPosition.y < -_outOfBoundsOffset || _updateOnlyDownPull == false && _scrollRect.content.anchoredPosition.y > yMaxPos+_outOfBoundsOffset;//y < 0 || y > 1;

            if (xOutOfBound || yOutOfBound)
            {
                OnGetOutOfBoundsRelease?.Invoke();
            }
        }
    }
}
