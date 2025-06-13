using System;
using System.Collections;
using DigitalRubyShared;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;
using Image = UnityEngine.UI.Image;

namespace UIManaging.Core
{
    public sealed class ScrollRectEnhancedScrollerSnapping : ScrollRectEx
    {
        private const float SNAP_TIME = 0.2f;
        private const EnhancedScroller.TweenType SNAP_TYPE = EnhancedScroller.TweenType.easeOutQuad;

        private EnhancedScroller _enhancedScroller;
        private int _startScrollIndex;

        [Inject] private FingersScript _fingersScript;
        private SwipeGestureRecognizer _swipeGesture;
        private PanGestureRecognizer _panGesture;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<int> Snapping;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        private int LastIndex => _enhancedScroller.NumberOfCells - 1;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void OnEnable()
        {
            base.OnEnable();

            // parent ScrollRect has [ExecuteAlways] class attribute, but FingersScripts is injected only during runtime
            if (!_fingersScript) return;
            
            _fingersScript.AddGesture(_swipeGesture);
            _fingersScript.AddGesture(_panGesture);
            _swipeGesture.StateUpdated += OnSwipeGestureStateUpdated;
            _panGesture.StateUpdated += OnPanGestureStateUpdated;
            _panGesture.AllowSimultaneousExecution(_swipeGesture);
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            
            if (!_fingersScript) return;
            
            _fingersScript.RemoveGesture(_swipeGesture);
            _fingersScript.RemoveGesture(_panGesture);
            _swipeGesture.StateUpdated -= OnSwipeGestureStateUpdated;
            _panGesture.StateUpdated -= OnPanGestureStateUpdated;
        }

        protected override void Awake()
        {
            base.Awake();
            
            horizontal = true;
            vertical = false;
            inertia = true;
            movementType = MovementType.Elastic;
                
            _enhancedScroller = GetComponent<EnhancedScroller>();
            _enhancedScroller.snapping = false;
            _enhancedScroller.snapSkipNext = false;

            PrepareGestures();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            var currentPosition = _enhancedScroller.ScrollPosition + (_enhancedScroller.ScrollRectSize * Mathf.Clamp01(_enhancedScroller.snapWatchOffset));
            _startScrollIndex = _enhancedScroller.GetCellViewIndexAtPosition(currentPosition);
        }

        public override void OnEndDrag(PointerEventData eventData)
        { 
            base.OnEndDrag(eventData);
            CheckSwipeOnOverlappingObject(eventData);
        }

        public void Snap(int index)
        {
            _enhancedScroller.JumpToDataIndex(index, _enhancedScroller.snapJumpToOffset, _enhancedScroller.snapCellCenterOffset, _enhancedScroller.snapUseCellSpacing, SNAP_TYPE, SNAP_TIME);
            Snapping?.Invoke(index);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void PrepareGestures()
        {
            _swipeGesture = new SwipeGestureRecognizer
            {
                MinimumNumberOfTouchesToTrack = 1,
                MinimumDistanceUnits = 0.1f,
                MinimumSpeedUnits = 1f,
                EndMode = SwipeGestureRecognizerEndMode.EndWhenTouchEnds
            };

            _panGesture = new PanGestureRecognizer();

            StartCoroutine(CreateGestureTarget());
        }

        private void SnapClosest()
        {
            var scrollOutOfBoundsRight = normalizedPosition.x > 1;
            var scrollOutOfBoundsLeft = normalizedPosition.x < 0;
            if (scrollOutOfBoundsRight || scrollOutOfBoundsLeft) return;
            
            _enhancedScroller.Snap();
            Snapping?.Invoke(_enhancedScroller.TargetSnapCellViewIndex);
        }

        private void SnapNext()
        {
            var snapIndex = Mathf.Clamp(_startScrollIndex + 1, 0, LastIndex);
            if (snapIndex == LastIndex && _startScrollIndex == LastIndex) return;
            
            Snap(snapIndex);
        }
        
        private void SnapPrevious()
        {
            var snapIndex = Mathf.Clamp(_startScrollIndex - 1, 0, LastIndex);
            if (snapIndex == 0 && _startScrollIndex == 0) return;
            
            Snap(snapIndex);
        }

        private void OnSwipeGestureStateUpdated(GestureRecognizer gesture)
        {
            if (_swipeGesture.State != GestureRecognizerState.Ended) return;
            OnSwipeGestureEnded();
        }
        
        private void OnPanGestureStateUpdated(GestureRecognizer gesture)
        {
            if (_panGesture.State != GestureRecognizerState.Ended) return;
            OnPanGestureEnded();
        }

        private void OnPanGestureEnded()
        {
            if (_enhancedScroller.IsTweening) return;
            SnapClosest();
        }

        private void OnSwipeGestureEnded()
        {
            switch (_swipeGesture.EndDirection)
            {
                case SwipeGestureRecognizerDirection.Left:
                    SnapNext();
                    break;
                case SwipeGestureRecognizerDirection.Right:
                    SnapPrevious();
                    break;
            }
        }

        //Create target for detecting gestures
        private IEnumerator CreateGestureTarget()
        {
            while (_enhancedScroller.Container == null)
            {
                yield return null;
            }
            
            var panel = new GameObject("GestureTarget", typeof(RectTransform));
            panel.transform.SetParent(transform);
            panel.transform.SetAsFirstSibling();
            
            var image = panel.AddComponent<Image>();
            image.color = new Color(1, 1, 1, 0);

            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(1, 1f);
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            rect.offsetMax = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            
            _swipeGesture.PlatformSpecificView = panel;
            _panGesture.PlatformSpecificView = panel;
        }

        private void CheckSwipeOnOverlappingObject(PointerEventData eventData)
        {
            if (_swipeGesture.State == GestureRecognizerState.Ended) return;

            var diraction = eventData.position - eventData.pressPosition;
            var absoluteX = Mathf.Abs(diraction.x);
            var absoluteY = Mathf.Abs(diraction.y);

            if (absoluteX > absoluteY && absoluteX > _swipeGesture.DirectionThreshold) 
            {
                if (diraction.x > 0)
                    SnapPrevious();
                if(diraction.x < 0)
                    SnapNext();
            }
        }
    }
}
