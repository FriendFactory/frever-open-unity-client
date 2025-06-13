using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRubyShared;
using JetBrains.Annotations;
using Modules.InputHandling.Gestures;
using UnityEngine.UI;
using Extensions;
using UnityEngine;

namespace Modules.InputHandling
{
    [UsedImplicitly]
    internal sealed class InputManager : IInputManager
    {
        private GestureType[] _allGestureTypes;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public bool Enabled => _fingersScript.enabled;

        private GestureType[] AllGestureTypes => _allGestureTypes ??= Enum.GetValues(typeof(GestureType)).Cast<GestureType>().ToArray();

        private readonly GestureType[] _simultaneousGestures = { GestureType.Pan, GestureType.Zoom };
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<bool> EnableStateChangeRequested;
        public event Action<float> PanHorizontalExecuting;
        public event Action<float> PanVerticalExecuting;
        public event Action<float> ZoomExecuting;
        public event Action ZoomBegin;
        public event Action PanHorizontalBegin;
        public event Action PanVerticalBegin;
        public event Action ZoomEnd;
        public event Action PanHorizontalEnd;
        public event Action PanVerticalEnd;
        public event Action<Vector2> RotationBegan;
        public event Action<float> RotationExecuted;
        public event Action RotationEnded;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        private readonly FingersScript _fingersScript;

        private readonly Dictionary<GestureType, GestureRecognizer> _gestures;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public InputManager(FingersScript fingersScript)
        {
            _fingersScript = fingersScript;
            _fingersScript.ComponentTypesToDenyPassThrough.Add(typeof(Image));
            
            var panGesture = new PanGestureRecognizer();
            var panHorizontalInput = new PanHorizontalInput(panGesture);
            var panVerticalInput = new PanVerticalInput(panGesture);
            var zoomInput = new ZoomInput();
            var rotationInput = new RotationInput();
            panGesture.AllowSimultaneousExecutionWithAllGestures();
            zoomInput.Gesture.AllowSimultaneousExecutionWithAllGestures();
            panHorizontalInput.GestureBegan += OnPanHorizontalBegin;
            panHorizontalInput.GestureExecuting += OnPanHorizontalExecuting;
            panHorizontalInput.GestureEnd += OnPanHorizontalEnd;
            
            panVerticalInput.GestureBegan += OnPanVerticalBegin;
            panVerticalInput.GestureExecuting += OnPanVerticalExecuting;
            panVerticalInput.GestureEnd += OnPanVerticalEnd;

            zoomInput.GestureBegan += OnZoomBegin;
            zoomInput.GestureExecuting += OnZoomExecuting;
            zoomInput.GestureEnd += OnZoomEnd;

            rotationInput.GestureBegan += OnRotationBegan;
            rotationInput.GestureExecuting += OnRotationExecuted;
            rotationInput.GestureEnd += OnRotationEnded;

            _gestures = new Dictionary<GestureType, GestureRecognizer>()
            {
                { GestureType.Pan, panGesture },
                { GestureType.Zoom, zoomInput.Gesture },
                { GestureType.Rotation, rotationInput.Gesture }
            };
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Enable(bool enable, params GestureType[] gestureTypes)
        {
            EnableStateChangeRequested?.Invoke(enable);

            if (gestureTypes.IsNullOrEmpty())
            {
                gestureTypes = AllGestureTypes;
            }
            
            foreach (var gestureType in gestureTypes)
            {
                var gesture = _gestures[gestureType];
                gesture.Reset();
                gesture.Enabled = enable;
                if (enable)
                {
                    _fingersScript.AddGesture(gesture);
                    if (_simultaneousGestures.Contains(gestureType))
                    {
                        gesture.AllowSimultaneousExecutionWithAllGestures();
                    }
                }
                else
                {
                    _fingersScript.RemoveGesture(gesture);
                }
            }

            _fingersScript.enabled = _gestures.Values.Any(x => x.Enabled);
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnPanHorizontalBegin()
        {
            PanHorizontalBegin?.Invoke();
        }
        
        private void OnPanVerticalBegin()
        {
            PanVerticalBegin?.Invoke();
        }
        
        private void OnZoomBegin()
        {
            ZoomBegin?.Invoke();
        }

        private void OnPanHorizontalExecuting(float value)
        {
            PanHorizontalExecuting?.Invoke(value);
        }
        
        private void OnPanVerticalExecuting(float value)
        {
            PanVerticalExecuting?.Invoke(value);
        }
        
        private void OnZoomExecuting(float value)
        {
            ZoomExecuting?.Invoke(value);
        }
        
        private void OnPanHorizontalEnd()
        {
            PanHorizontalEnd?.Invoke();
        }
        
        private void OnPanVerticalEnd()
        {
            PanVerticalEnd?.Invoke();
        }
        
        private void OnZoomEnd()
        {
            ZoomEnd?.Invoke();
        }

        private void OnRotationBegan()
        {
            var rotationPivot = Input.mousePosition;
            RotationBegan?.Invoke(rotationPivot);
        }

        private void OnRotationExecuted(float deltaDegrees)
        {
            RotationExecuted?.Invoke(deltaDegrees);
        }
        
        private void OnRotationEnded()
        {
            RotationEnded?.Invoke();
        }
    }
}
