using Extensions;
using UnityEngine;

namespace UIManaging.Pages.PublishPage
{
    internal sealed class InputTextOverlay: MonoBehaviour
    {
        [SerializeField] private RectTransform _target;
        [SerializeField] private RectTransform _upperPart;
        [SerializeField] private RectTransform _lowerPart;

        private void OnEnable()
        {
            Refresh();
        }

        public void SetTarget(RectTransform target)
        {
            _target = target;
        }

        [ContextMenu("Refresh")]
        private void Refresh()
        {
            if (_target == null) return;
            var canvas = _target.GetComponentInParent<Canvas>();
            var corners = new Vector3[4];
            canvas.GetComponent<RectTransform>().GetWorldCorners(corners);
            
            var topOfCanvas = corners[1].y;

            _target.GetWorldCorners(corners);
            var topOfTarget = corners[1].y;
            var bottomOfTarget = corners[0].y;

            var freeSpaceAboveTarget = (topOfCanvas - topOfTarget) / canvas.scaleFactor;
            var freeSpaceBelowTarget = bottomOfTarget / canvas.scaleFactor;

            SetHeight(_upperPart, freeSpaceAboveTarget);
            SetHeight(_lowerPart, freeSpaceBelowTarget);
            
            _upperPart.SetPositionY((topOfCanvas + topOfTarget)/2f);
            _lowerPart.SetPositionY(bottomOfTarget / 2f);
        }

        private void SetHeight(RectTransform rectTransform, float height)
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, height);
        }
    }
}