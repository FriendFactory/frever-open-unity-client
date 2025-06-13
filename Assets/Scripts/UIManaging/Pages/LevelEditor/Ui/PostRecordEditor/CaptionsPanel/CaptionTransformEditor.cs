using System;
using System.Collections.Generic;
using Extensions;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel
{
    internal sealed class CaptionTransformEditor : MonoBehaviour
    {
        [SerializeField] private EditableCaptionView _captionView;
        [SerializeField] private CaptionPositionControl _captionPositionControl;
        [SerializeField] private PositionAdjuster _positionAdjuster;
        [SerializeField] private RotationAdjuster _rotationAdjuster;
       
        private readonly List<Touch> _lastFrameTouches = new();
        private Vector2 _touchToCaptionStartDistance;
        private float _previousAngle;
        private Vector2 _captionRawPosition;
        private Vector2 _captionNormalizedPosBetweenTouches;
        private RectTransform _viewPort;
        private bool _isRunning;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action Completed;
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(RectTransform viewPort)
        {
            _viewPort = viewPort;
        }
        
        public void Run()
        {
            _lastFrameTouches.Clear();
            _positionAdjuster.SetActive(true);
            _rotationAdjuster.SetActive(true);
            _rotationAdjuster.SetInitialRotation(_captionView.RotationEulerAngle);
            _positionAdjuster.Init(_viewPort);
            _isRunning = true;
        }
        
        public void Stop()
        {
            _isRunning = false;
            _positionAdjuster.SetActive(false);
            _rotationAdjuster.SetActive(false);
        }
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _positionAdjuster.SetActive(false);
            _rotationAdjuster.SetActive(false);
        }

        private void LateUpdate()
        {
            if (!_isRunning) return;
            
            var touchCount = GetTouchCount();
            var gestureChanged = _lastFrameTouches.Count != touchCount && touchCount > 0;
            if (gestureChanged)
            {
                SaveCurrentDistanceToCaption();
            }
            
            switch (touchCount)
            {
                case 0:
                    _lastFrameTouches.Clear();
                    OnComplete();
                    return;
                case 1:
                    HandleSingleTouch();
                    break;
                case 2:
                    HandleMultitouch();
                    break;
            }

            _lastFrameTouches.Clear();
            _lastFrameTouches.AddRange(GetTouches());
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnComplete()
        {
            Stop();
            Completed?.Invoke();
        }
        
        private void HandleSingleTouch()
        {
            var touch = GetTouch(0);
            _captionRawPosition = touch.position - _touchToCaptionStartDistance;
            var adjustedPos = _positionAdjuster.GetAdjustedPosition(_captionRawPosition);
            _captionPositionControl.TryToSetPosition(adjustedPos);
        }
        
        private void HandleMultitouch()
        {
            var isGestureJustStarted = _lastFrameTouches.Count != 2;
            if (isGestureJustStarted)
            {
                _previousAngle = GetCurrentAngle();
                return;
            }

            var currentAngle = GetCurrentAngle();
            var deltaAngle = currentAngle - _previousAngle;
            _rotationAdjuster.AddRotation(deltaAngle);
            _captionView.SetRotation(_rotationAdjuster.AdjustedRotation);
            _previousAngle = currentAngle;

            float GetCurrentAngle()
            {
                return Vector2.SignedAngle(Vector2.up, GetTouch(0).position - GetTouch(1).position);
            }
        }

        private void SaveCurrentDistanceToCaption()
        {
            _touchToCaptionStartDistance = GetTouch(0).position - _captionPositionControl.CurrentPosition;
        }

        private static int GetTouchCount()
        {
            #if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isRemoteConnected)
                return Input.GetMouseButton(0) ? 1 : 0;
            #endif
            return Input.touchCount;
        }

        private static IEnumerable<Touch> GetTouches()
        {
            var touchCount = GetTouchCount();
            var output = new Touch[touchCount];
            for (var i = 0; i < touchCount; i++)
            {
                output[i] = GetTouch(i);
            }
            return output;
        }
        
        private static Touch GetTouch(int index)
        {
            #if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isRemoteConnected)
                return new Touch
                {
                    position = Input.mousePosition,
                    fingerId = index
                };
            #endif
            return Input.GetTouch(index);
        }

    }
}