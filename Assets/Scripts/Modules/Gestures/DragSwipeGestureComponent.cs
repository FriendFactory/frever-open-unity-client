using System;
using System.Diagnostics;
using Abstract;
using DigitalRubyShared;
using UnityEngine;

namespace Modules.Gestures
{
    public class DragSwipeGestureComponent : MonoBehaviour, IInitializable
    {
        [SerializeField] private PanGestureRecognizerComponentScript _panGesture;
        [SerializeField] private RectTransform _target;
        [Header("Settings")] 
        [SerializeField] private Vector2 _boundsY = new Vector2(100f, 150f);
        [SerializeField] private float _swipeAwaySpeed = 15f;
        [SerializeField] private float _smoothTime = 0.05f;
        [Header("Tap")] 
        [SerializeField] private float _tapThresholdTime = 0.25f;

        private Vector2 _initialPosition;
        private Vector2 _dragOffset;
        private Vector2 _currentVelocity;
        private Stopwatch _stopwatch;
        
        public event Action SwipeUp;
        public event Action Tap;

        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            _initialPosition = _target.position;
            
            _panGesture.GestureStateUpdated.AddListener(OnPanGestureStateUpdated);
            
            IsInitialized = true;
        }

        public void CleanUp()
        {
            _panGesture.GestureStateUpdated.RemoveListener(OnPanGestureStateUpdated);

            IsInitialized = false;
        }

        private void OnPanGestureStateUpdated(GestureRecognizer gesture)
        {
            switch (gesture.State)
            {
                case GestureRecognizerState.Began:
                    OnGestureBegan();
                    break;
                case GestureRecognizerState.Executing:
                    OnGestureExecuting();
                    break;
                case GestureRecognizerState.Ended:
                    OnGestureEnded();
                    break;
                case GestureRecognizerState.Failed:
                    _stopwatch.Stop();
                    break;
            }

            void OnGestureBegan()
            {
                var screenPoint = new Vector2(gesture.FocusX, gesture.FocusY);
                var dragPosition = GetDragPosition(screenPoint);

                _dragOffset = (Vector2)_target.position - dragPosition;

                _stopwatch = new Stopwatch();
                _stopwatch.Restart();
            }

            void OnGestureExecuting()
            {
                var screenPoint = new Vector2(gesture.FocusX, gesture.FocusY);
                var dragPosition = GetDragPosition(screenPoint);
                var currentPosition = (Vector2)_target.position;
                var targetPosition = currentPosition;
                targetPosition.y = dragPosition.y + _dragOffset.y;
                var halfHeight = _target.rect.height * 0.5f;

                var distanceY = targetPosition.y - _initialPosition.y + halfHeight;
                var speed = new Vector2(gesture.VelocityXUnits, gesture.VelocityYUnits).magnitude;
                var distanceCondition = distanceY >= _boundsY.x;
                var swipeSpeedCondition = gesture.VelocityYUnits > 0 && speed > _swipeAwaySpeed;

                if (distanceCondition || swipeSpeedCondition)
                {
                    SwipeUp?.Invoke();

                    gesture.Reset();
                    _stopwatch?.Stop();
                    return;
                }

                // apply min y position constraint
                var minY = _initialPosition.y - _boundsY.y + halfHeight;
                targetPosition.y = Mathf.Max(targetPosition.y, minY);

                _target.position = Vector2.SmoothDamp(currentPosition, targetPosition, ref _currentVelocity, _smoothTime);
            }

            void OnGestureEnded()
            {
                if (_stopwatch == null) return;
                
                var gestureTime = _stopwatch.Elapsed.TotalSeconds; 
                
                _stopwatch.Stop();
                
                if (gestureTime <= _tapThresholdTime)
                {
                    Tap?.Invoke();
                }
            }
        }

        private Vector2 GetDragPosition(Vector2 screenPoint)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_target, screenPoint, null, out var localPoint);
            var dragPosition = _target.TransformPoint(localPoint);

            return dragPosition;
        }
    }
}