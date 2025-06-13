using System;
using System.Collections.Generic;
using Extensions;
using UnityEngine;

namespace UIManaging.Pages.UserProfile.Ui
{
    public class ProfileCoverPhotoAnimator : MonoBehaviour
    {
        [SerializeField] private ProfileScrollablePanel _scrollablePanel;
        [Space]
        [SerializeField] private List<RectTransform> _objectsToMove;
        [Space]
        [SerializeField] private List<ResizableRectTransform> _objectsToResize;

        private float _startScrollPosition;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public Dictionary<RectTransform, Vector2> StartPositions { get; } = new Dictionary<RectTransform, Vector2>();
        public Dictionary<RectTransform, Vector2> StartSizes { get; } = new Dictionary<RectTransform, Vector2>();

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Start()
        {
            _startScrollPosition = -_scrollablePanel.GetComponent<RectTransform>().GetTop();

            foreach (var item in _objectsToMove)
            {
                StartPositions.Add(item, item.anchoredPosition);
            }

            foreach (var item in _objectsToResize)
            {
                StartSizes.Add(item.RectTransform, item.RectTransform.sizeDelta);
            }
        }

        private void OnEnable()
        {
            _scrollablePanel.OnDragEvent += Animate;
        }

        private void OnDisable()
        {
            _scrollablePanel.OnDragEvent -= Animate;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void Animate(float scrollChange)
        {
            var delta = _startScrollPosition - scrollChange;
            if (delta < 0) return;

            foreach (var item in _objectsToMove)
            {
                var startPos = StartPositions[item];
                item.anchoredPosition = new Vector2(startPos.x, startPos.y + delta);
            }
            foreach (var item in _objectsToResize)
            {
                var startDelta = StartSizes[item.RectTransform];
                item.RectTransform.sizeDelta = new Vector2(startDelta.x, startDelta.y + delta/ item.SizeMoveDifference);
            }
        }

        //---------------------------------------------------------------------
        // Nested
        //---------------------------------------------------------------------

        [Serializable]
        internal class ResizableRectTransform
        {
            public RectTransform RectTransform;
            public float SizeMoveDifference = 1f;
        }
    }
}