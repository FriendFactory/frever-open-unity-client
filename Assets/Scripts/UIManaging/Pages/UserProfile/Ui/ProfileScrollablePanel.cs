using System;
using DG.Tweening;
using Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIManaging.Pages.UserProfile.Ui
{
    public class ProfileScrollablePanel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Required]
        [SerializeField] private RectTransform _scrollablePart;
        [Required]
        [SerializeField] private RectTransform _videoTabs;
        [Space]
        [SerializeField] private float _scrollThreshold = 50f;
        [SerializeField] private float _tweenToPosSpeed = 2000f;
        [SerializeField] private float _scrollMultiplier = 1f;
        [SerializeField] private float _maxOffset;

        private float _initialScrollablePartOffset;
        private Tween _autoScrollTween;
        private Vector2 _defaultSizeDelta;
        private Vector2 _expandedSizeDelta;
        private float _topYPosition;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action OnIsScrollablePartOnTopChangedEvent;
        public event Action<float> OnDragEvent;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public bool IsExpanded { get; private set; }
        public float DefaultTopYPosition { get; private set; }

        public float TopYPosition
        {
            get => _topYPosition;
            set
            {
                _topYPosition = value;
                _expandedSizeDelta = new Vector2(_scrollablePart.sizeDelta.x, _topYPosition);
            }
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _initialScrollablePartOffset = -_scrollablePart.GetTop();
            DefaultTopYPosition = _videoTabs.GetTop() - _scrollablePart.GetTop();
            _topYPosition = DefaultTopYPosition;

            var scrollableWidth = _scrollablePart.sizeDelta.x;
            _defaultSizeDelta = new Vector2(scrollableWidth, _initialScrollablePartOffset);
            _expandedSizeDelta = new Vector2(scrollableWidth, _topYPosition);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void ResetToInitialState()
        {
            var targetSizeDelta = new Vector2(_scrollablePart.sizeDelta.x, _initialScrollablePartOffset);
            _scrollablePart.sizeDelta = targetSizeDelta;
            SetIsScrollablePartOnTop(false);
        }

        //---------------------------------------------------------------------
        // IDragHandler
        //---------------------------------------------------------------------

        public void OnBeginDrag(PointerEventData eventData)
        {
            _autoScrollTween?.Kill();
            AutoScrollUpdate();
        }

        public void OnDrag(PointerEventData eventData)
        {
            var newValue = _scrollablePart.sizeDelta.y + eventData.delta.y * _scrollMultiplier;
            newValue = Mathf.Clamp(newValue, _maxOffset, TopYPosition);
            _scrollablePart.sizeDelta = new Vector2(_scrollablePart.sizeDelta.x, newValue);
            AutoScrollUpdate();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            var currentOffset = _scrollablePart.sizeDelta.y;

            if (IsExpanded && currentOffset < TopYPosition - _scrollThreshold)
            {
                IsExpanded = false;
            }
            else if (!IsExpanded && currentOffset > _initialScrollablePartOffset + _scrollThreshold)
            {
                IsExpanded = true;
            }

            PlayTween();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void PlayTween()
        {
            var targetSizeDelta = IsExpanded
                ? _expandedSizeDelta
                : _defaultSizeDelta;

            _autoScrollTween?.Kill();

            _autoScrollTween = _scrollablePart?.DOSizeDelta(targetSizeDelta, _tweenToPosSpeed).SetSpeedBased().SetEase(Ease.OutQuad)
               .OnUpdate(AutoScrollUpdate)
               .OnComplete(() =>
                {
                    AutoScrollUpdate();
                    SetIsScrollablePartOnTop(IsExpanded);
                });

            AutoScrollUpdate();
        }

        private void AutoScrollUpdate()
        {
            OnDragEvent?.Invoke(_scrollablePart.sizeDelta.y);
        }

        private void SetIsScrollablePartOnTop(bool value)
        {
            IsExpanded = value;
            OnIsScrollablePartOnTopChangedEvent?.Invoke();
        }
    }
}