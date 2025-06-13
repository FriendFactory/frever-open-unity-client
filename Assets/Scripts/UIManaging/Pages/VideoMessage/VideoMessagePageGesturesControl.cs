using System;
using DigitalRubyShared;
using JetBrains.Annotations;
using Modules.InputHandling;
using UnityEngine;

namespace UIManaging.Pages.VideoMessage
{
    internal interface IVideoMessagePageGesturesControl: IRotationGestureSource
    {
        event Action<PanGestureRecognizer> PanGestureStateChanged;
        event Action<ScaleGestureRecognizer> ZoomGestureStateChanged;

        void Enable(RectTransform viewPort);
        void Disable();
    }
    
    [UsedImplicitly]
    internal sealed class VideoMessagePageGesturesControl: IVideoMessagePageGesturesControl
    {
        private readonly FingersScript _fingersScript;
        private readonly PanGestureRecognizer _panGestureRecognizer = new();
        private readonly ScaleGestureRecognizer _scaleGestureRecognizer = new();
        private readonly RotateGestureRecognizer _rotateGestureRecognizer = new();

        private readonly GestureRecognizer[] _gestureRecognizers;

        private float _rotationDelta;
        private bool _enabled;
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<PanGestureRecognizer> PanGestureStateChanged;
        public event Action<ScaleGestureRecognizer> ZoomGestureStateChanged;
        public event Action<Vector2> RotationBegan;
        public event Action<float> RotationExecuted;
        public event Action RotationEnded;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public VideoMessagePageGesturesControl(FingersScript fingersScript)
        {
            _fingersScript = fingersScript;
            _gestureRecognizers = new GestureRecognizer[] { _panGestureRecognizer, _scaleGestureRecognizer, _rotateGestureRecognizer };
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Enable(RectTransform viewPort)
        {
            if (_enabled) return;
            foreach (var recognizer in _gestureRecognizers)
            {
                recognizer.Reset();
                recognizer.Enabled = true;
                recognizer.PlatformSpecificView = viewPort;
                _fingersScript.AddGesture(recognizer);
            }

            _panGestureRecognizer.ThresholdUnits = 0.1f;
            _panGestureRecognizer.StateUpdated += OnPanGestureStateUpdated;
            _scaleGestureRecognizer.ThresholdUnits = 0.1f;
            _scaleGestureRecognizer.StateUpdated += OnZoomStateUpdated;
            _rotateGestureRecognizer.StateUpdated += OnRotationGesture;
            _scaleGestureRecognizer.AllowSimultaneousExecutionWithAllGestures();
            _rotateGestureRecognizer.AllowSimultaneousExecutionWithAllGestures();
            
            _enabled = true;
        }

        public void Disable()
        {
            if (!_enabled) return;
            foreach (var recognizer in _gestureRecognizers)
            {
                recognizer.Reset();
                recognizer.Enabled = false;
                _fingersScript.RemoveGesture(recognizer);
            }
            _panGestureRecognizer.StateUpdated -= OnPanGestureStateUpdated;
            _scaleGestureRecognizer.StateUpdated -= OnZoomStateUpdated;
            _rotateGestureRecognizer.StateUpdated -= OnRotationGesture;
            _enabled = false;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnRotationGesture(GestureRecognizer gesture)
        {
            switch (gesture.State)
            {
                case GestureRecognizerState.Began:
                    RotationBegan?.Invoke(Input.mousePosition);
                    break;
                case GestureRecognizerState.Executing:
                    _rotationDelta = ((RotateGestureRecognizer)gesture).RotationDegreesDelta;
                    RotationExecuted?.Invoke(_rotationDelta);
                    break;
                case GestureRecognizerState.Ended:
                    RotationEnded?.Invoke();
                    break;
                default:
                    return;
            }
        }

        private void OnPanGestureStateUpdated(GestureRecognizer gesture)
        {
            PanGestureStateChanged?.Invoke(gesture as PanGestureRecognizer);
        }
        
        private void OnZoomStateUpdated(GestureRecognizer gesture)
        {
            ZoomGestureStateChanged?.Invoke(gesture as ScaleGestureRecognizer);
        }
    }
}