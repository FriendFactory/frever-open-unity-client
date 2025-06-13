using System;
using System.Collections.Generic;
using System.Linq;
using Abstract;
using DG.Tweening;
using Extensions;
using Modules.VideoStreaming.Feed;
using UnityEngine;
using UnityEngine.EventSystems;
using Tween = DG.Tweening.Tween;

namespace UIManaging.Pages.Feed.Core
{
    public sealed class FeedScrollView : BaseContextDataView<FeedScrollController>, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        private const int TOUCHES_LIMIT = 1;
        private const float SNAP_DURATION = 0.3f;

        [SerializeField] private float _scrollThreshold = 6f;
        [SerializeField] private float _refreshThreshold = 350f;
        [SerializeField] private RectTransform _prefabsParent;
        [SerializeField] private FeedRefreshIndicator _feedRefreshIndicator;
        [SerializeField] private CanvasGroup _feedPanel;

        private float _scaleFactor;
        private float _scaledScrollThreshold;
        private float _scaledRefreshThreshold;

        private bool _isDragging;
        private int _selectedViewPosition = FeedScrollController.VISIBLE_POSITION;
        private bool _isDraggingRefreshIndicator;
        private bool _draggingDown;
        private int _pointerId;

        private readonly List<Tween> _snappingTweens = new List<Tween>();

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private List<RectTransform> ViewsRects { get; } = new List<RectTransform>();

        public RectTransform PrefabsParent => _prefabsParent;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action OnViewScrolledUpEvent;
        public event Action OnViewScrolledDownEvent;
        internal event Action<bool> ViewRecycled;
        public event Action OnRefreshDragPerformedEvent;
        public event Action OnScrolledNextOnBottom;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _scaleFactor = GetComponentInParent<Canvas>().scaleFactor;
            _scaledScrollThreshold = _scrollThreshold * _scaleFactor;
            _scaledRefreshThreshold = _refreshThreshold * _scaleFactor;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != _pointerId) return;

            _isDragging = true;
            _draggingDown = eventData.delta.y < 0f;
            if (!_draggingDown && !ContextData.AllowToScrollDown())
            {
                _isDragging = false;
                return;
            }

            if (_draggingDown && !ContextData.AllowToScrollUp())
            {
                _isDragging = false;
                return;
            }
            
            _isDragging = true;

            _feedPanel.interactable = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != _pointerId) return;
            
            if(!_isDragging) return;
            var dragDelta = eventData.pressPosition.y - eventData.position.y;
            var offset = eventData.delta.y / _scaleFactor;
            var draggingDown = eventData.delta.y < 0f;
            var check = (!_draggingDown && draggingDown);

            if (!check)
            {
                if (IsTopPosition())
                {
                    if (draggingDown || _isDraggingRefreshIndicator)
                    {
                        _isDraggingRefreshIndicator = true;
                        var newRefreshIndicatorYPosition =
                            Math.Max(_feedRefreshIndicator.RectTransform.anchoredPosition.y + offset,
                                     -_refreshThreshold);
                        _feedRefreshIndicator.RectTransform.anchoredPosition = new Vector2(
                            _feedRefreshIndicator.RectTransform.anchoredPosition.x, newRefreshIndicatorYPosition);
                    }
                }

                if (_isDraggingRefreshIndicator)
                {
                    var lerp = Mathf.Clamp01(dragDelta / _scaledRefreshThreshold);
                    _feedRefreshIndicator.SetNormalizedArrowPosition(lerp);
                    return;
                }
            }

            offset = GetFixedTopOffset(offset);
            offset = GetFixedBottomOffset(offset);

            foreach (var viewRect in ViewsRects)
            {
                viewRect.anchoredPosition = new Vector2(viewRect.anchoredPosition.x, viewRect.anchoredPosition.y + offset);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if(!_isDragging) return;
            if (eventData.pointerId != _pointerId) return;
            
            _isDragging = false;
            _feedPanel.interactable = true;

            var dragDelta = eventData.pressPosition.y - eventData.position.y;
            _feedRefreshIndicator.ResetToInitialPosition();

            if (IsTopPosition() && dragDelta >= _scaledRefreshThreshold && _isDraggingRefreshIndicator)
            {
                OnRefreshDragPerformedEvent?.Invoke();
            }

            if (_isDraggingRefreshIndicator)
            {
                _isDraggingRefreshIndicator = false;
                return;
            }

            if (dragDelta > _scaledScrollThreshold)
            {
                if (!IsTopPosition() && ContextData.AllowToScrollUp())
                {
                    ScrollUp();
                }
            }
            else if (dragDelta < -_scaledScrollThreshold)
            {
                if (!IsBottomPosition() && ContextData.AllowToScrollDown())
                {
                    ScrollDown();
                }
                else if(IsBottomPosition())
                {
                    OnScrolledNextOnBottom?.Invoke();
                }
            }

            StartSnapping();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public void ScrollDown()
        {
            _selectedViewPosition++;
            ContextData.Index += 1;

            if (_selectedViewPosition > FeedScrollController.LAST_POSITION)
            {
                _selectedViewPosition = FeedScrollController.LAST_POSITION;
            }

            TryRepositionView();
            OnViewScrolledDownEvent?.Invoke();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public void ScrollUp()
        {
            _selectedViewPosition--;
            ContextData.Index -= 1;

            if (_selectedViewPosition < 0)
            {
                _selectedViewPosition = 0;
            }

            TryRepositionView();
            OnViewScrolledUpEvent?.Invoke();
        }
        
        public bool IsBottomPosition()
        {
            return ContextData.Index >= ContextData.Amount - 1;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            ContextData.AssignPointerCallbacks(OnPointerDown, OnPointerUp);
            Clear();
            SpawnRects();
        }

        protected override void BeforeCleanup()
        {
            Clear();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void Clear()
        {
            if (ContextData == null)
            {
                return;
            }

            for (var i = FeedScrollController.LAST_POSITION; i >= 0; i--)
            {
                ContextData.RecycleByIndex(i);
            }
            
            ViewsRects.Clear();
        }
        
        private void SpawnRects()
        {
            for (var i = 0; i < FeedScrollController.VIEWS_POOL_CAPACITY; i++)
            {
                ViewsRects.Add(ContextData.GetInstanceByIndex(i, _prefabsParent));
                ViewsRects[i].SetSiblingIndex(i);
            }
        }

        private void TryRepositionView()
        {
            if (_selectedViewPosition > FeedScrollController.VISIBLE_POSITION)
            {
                MoveTopViewToTheEnd();
            }
            else
            {
                MoveBottomViewOnTop();
            }

            _selectedViewPosition = FeedScrollController.VISIBLE_POSITION;
        }

        private void MoveBottomViewOnTop()
        {
            ContextData.RecycleByIndex(FeedScrollController.LAST_POSITION);
            ViewsRects.RemoveAt(FeedScrollController.LAST_POSITION);
            ViewsRects.Insert(0, ContextData.GetInstanceByIndex(0, _prefabsParent));
            ViewsRects[0].anchoredPosition = (FeedScrollController.VISIBLE_POSITION + 1) 
                                           * Vector2.up * ViewsRects[0].rect.size.y;
            ViewRecycled?.Invoke(true);
        }

        private void MoveTopViewToTheEnd()
        {
            ContextData.RecycleByIndex(0);
            ViewsRects.RemoveAt(0);
            ViewsRects.Add(ContextData.GetInstanceByIndex(FeedScrollController.LAST_POSITION, _prefabsParent));
            ViewsRects[FeedScrollController.LAST_POSITION].anchoredPosition = (FeedScrollController.VISIBLE_POSITION - FeedScrollController.LAST_POSITION - 1) 
                                                                           * Vector2.up * ViewsRects[FeedScrollController.LAST_POSITION].rect.size.y;
            ViewRecycled?.Invoke(false);
        }

        private float GetFixedBottomOffset(float inputOffset)
        {
            if (!IsBottomPosition()) return inputOffset;

            var mostBottomView = ViewsRects.OrderByDescending(pg => pg.anchoredPosition.y).Last();
            var newMostBottomViewPosition = mostBottomView.anchoredPosition.y + inputOffset;
            var distance = -_prefabsParent.rect.size.y - newMostBottomViewPosition;

            if (inputOffset > distance)
            {
                inputOffset += distance;
            }

            return inputOffset;
        }

        private float GetFixedTopOffset(float inputOffset)
        {
            if (!IsTopPosition()) return inputOffset;

            var mostTopView = ViewsRects.OrderByDescending(pg => pg.anchoredPosition.y).First();
            var newMostTopViewPosition = mostTopView.anchoredPosition.y + inputOffset;
            var distance = _prefabsParent.rect.size.y - newMostTopViewPosition;

            if (inputOffset < distance)
            {
                inputOffset += distance;
            }

            return inputOffset;
        }

        private bool IsTopPosition()
        {
            return ContextData?.Index <= 0;
        }
        
        public void OnPointerDown(int pointerId)
        {
            if (Input.touchCount > TOUCHES_LIMIT) return;

            _pointerId = pointerId;
            PauseSnapping();
        }

        public void OnPointerUp(int pointerId)
        {
            if (pointerId != _pointerId) return;
            ContinueSnapping();
        }

        private void StartSnapping()
        {
            ClearSnappingTweens();
            
            for (var i = 0; i < ViewsRects.Count; i++)
            {
                var tween = ViewsRects[i].DOAnchorPos(
                    (FeedScrollController.VISIBLE_POSITION - i) *  Vector2.up * _prefabsParent.rect.size.y, 
                    SNAP_DURATION).SetEase(Ease.OutQuad);
                _snappingTweens.Add(tween);
            }
        }

        private void PauseSnapping()
        {
            foreach (var tween in _snappingTweens)
            {
                tween.Pause();
            }
        }
        
        private void ContinueSnapping()
        {
            foreach (var tween in _snappingTweens)
            {
                tween.Play();
            }
        }

        private void ClearSnappingTweens()
        {
            foreach (var tween in _snappingTweens)
            {
                tween.Kill();
            } 
            
            _snappingTweens.Clear();
        }
    }
}