using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.UI.Extensions;
using UnityEngine.EventSystems;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    public class AdjustmentItem : AdjustmentItemBase, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField]
        private ScrollRect _scrollbar;
        [SerializeField]
        private float _threshold = 0.01f;

        private float _startPoint;
        private float _endPoint;

        void Start()
        {
            _scrollbar.onValueChanged.AddListener(OnValueChanged);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _startPoint = Mathf.Clamp(_scrollbar.horizontalNormalizedPosition, 0, 1);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _endPoint = Mathf.Clamp(_scrollbar.horizontalNormalizedPosition, 0, 1);
            RaiseAdjustmentChangedDiff(_startPoint, _endPoint);
        }

        public override void SetDefaultValue(float value)
        {
            base.SetDefaultValue(value);
            var rectTransform = _newIcon.transform as RectTransform;
            var parentTransform = rectTransform.parent as RectTransform;
            rectTransform.anchoredPosition = new Vector2(value * parentTransform.rect.width, rectTransform.anchoredPosition.y);
        }

        public override void SetValue(float value)
        {
            _notUserChange = true;
            _scrollbar.normalizedPosition = new Vector2(value, 0);
            _notUserChange = false;
        }

        public void SetParentScrollRect(ScrollRect parentScroll)
        {
            var scrollConflictManager = _scrollbar.GetComponent<ScrollConflictManager>();
            scrollConflictManager.ParentScrollRect = parentScroll;
        }

        private void OnValueChanged(Vector2 position)
        {
            if (_notUserChange) return;

            var value = Mathf.Clamp(position.x, 0, 1);

            if (Mathf.Abs(value - _previousValue) < _threshold)
            {
                return;
            }

            RaiseAdjustmentChanged(value);
            _previousValue = position.x;
        }

        private void OnDrag(PointerEventData eventData)
        {
            var currentPoint = Mathf.Clamp(_scrollbar.horizontalNormalizedPosition, 0, 1);
            if (Mathf.Abs(currentPoint - _startPoint) > _threshold && Math.Abs(currentPoint - _defaultValue) < _threshold)
            {
                Handheld.Vibrate();
            }
        }
    }
}