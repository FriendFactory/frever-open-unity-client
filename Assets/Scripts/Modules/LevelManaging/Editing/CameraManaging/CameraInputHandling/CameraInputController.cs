using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Modules.CameraSystem.CameraAnimations;
using Modules.CameraSystem.CameraSystemCore;
using Modules.InputHandling;
using UnityEngine;
using Zenject;

namespace Modules.LevelManaging.Editing.CameraManaging.CameraInputHandling
{
    /// <summary>
    /// Overrides camera animation values based on user input
    /// </summary>
    [UsedImplicitly]
    internal sealed class CameraInputController : ICameraInputController, ILateTickable
    {
        private const float MAX_Y_AXIS = 1f;
        private const float MIN_Y_AXIS = 0f;
        
        private readonly IInputManager _inputManager;
        private readonly ICameraSystem _cameraSystem;
        private readonly ICameraTemplatesManager _cameraTemplatesManager;

        private readonly Dictionary<CameraAnimationProperty, float> _cameraPropertyChanges =
            new Dictionary<CameraAnimationProperty, float>();

        private readonly HashSet<CameraAnimationProperty> _updatingProperties = new HashSet<CameraAnimationProperty>();

        private bool _autoActivation;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public bool AutoActivateOnCameraSystemActivation
        {
            get => _autoActivation;
            set
            {
                if(_autoActivation == value) return;
                _autoActivation = value;
                if (value)
                {
                    _cameraSystem.EnabledStatusChanged += Activate;
                }
                else
                {
                    _cameraSystem.EnabledStatusChanged -= Activate;
                }
            }
        }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<float> OrbitRadiusUpdated;
        public event Action OrbitRadiusFinishedUpdating;
        public event Action CameraModificationStarted;
        public event Action CameraModificationCompleted;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public CameraInputController(IInputManager inputManager, ICameraSystem cameraSystem, ICameraTemplatesManager cameraTemplatesManager)
        {
            _inputManager = inputManager;
            _cameraSystem = cameraSystem;
            _cameraTemplatesManager = cameraTemplatesManager;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Activate(bool activate)
        {
            _cameraPropertyChanges.Clear();
            _updatingProperties.Clear();
            UnSubscribeEvents();
            if (activate) SubscribeEvents();
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        public void LateTick()
        {
            if(_cameraPropertyChanges.Count == 0) return;
            
            foreach (var propertyChange in _cameraPropertyChanges)
            {
                _cameraSystem.Set(propertyChange.Key, propertyChange.Value);
            }
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnPanHorizontalBegan()
        {
            RememberStartPropertyValue(CameraAnimationProperty.AxisX);
            InitUpdating(CameraAnimationProperty.AxisX);
            OnCameraModificationStarted();
        }

        private void OnPanVerticalBegan()
        {
            RememberStartPropertyValue(CameraAnimationProperty.AxisY);
            InitUpdating(CameraAnimationProperty.AxisY);
            OnCameraModificationStarted();
        }

        private void OnZoomBegan()
        {
            RememberStartPropertyValue(CameraAnimationProperty.OrbitRadius);
            InitUpdating(CameraAnimationProperty.OrbitRadius);
            OnCameraModificationStarted();
        }

        private void OnPanHorizontalExecuting(float value)
        {
            if (!IsUpdating(CameraAnimationProperty.AxisX)) return;
            
            var xAxis = GetValue(CameraAnimationProperty.AxisX);
            xAxis += value;
            SetValue(CameraAnimationProperty.AxisX, xAxis);
        }

        private void OnPanVerticalExecuting(float value)
        { 
            if (!IsUpdating(CameraAnimationProperty.AxisY)) return;
            
            var yAxis = GetValue(CameraAnimationProperty.AxisY);
            yAxis += value;
            yAxis = Mathf.Clamp(yAxis, MIN_Y_AXIS, MAX_Y_AXIS);
            SetValue(CameraAnimationProperty.AxisY, yAxis);
        }

        private void OnZoomExecuting(float value)
        {
            if (!IsUpdating(CameraAnimationProperty.OrbitRadius)) return;
            
            var currentCameraSettings = _cameraSystem.GetCurrentCameraSetting();
            var minValue = currentCameraSettings.OrbitRadiusMin;
            var maxValue = currentCameraSettings.OrbitRadiusMax;

            var currentValue = GetValue(CameraAnimationProperty.OrbitRadius);
            
            var newValue = Mathf.Clamp(currentValue * value, minValue, maxValue);
            SetValue(CameraAnimationProperty.OrbitRadius, newValue);
            OrbitRadiusUpdated?.Invoke(newValue);
        }

        private void OnCameraModificationStarted()
        {
            CameraModificationStarted?.Invoke();
        }
        
        private void OnCameraStateModifiedCompleted()
        {
            OnCameraStateChanged();
            CameraModificationCompleted?.Invoke();
        }

        private void OnZoomEnded()
        {
            if (!IsUpdating(CameraAnimationProperty.OrbitRadius)) return;
            StopUpdating(CameraAnimationProperty.OrbitRadius);

            OnCameraStateChanged();
            OrbitRadiusFinishedUpdating?.Invoke();
            CameraModificationCompleted?.Invoke();
        }

        private void SubscribeEvents()
        {
            _inputManager.PanHorizontalBegin += OnPanHorizontalBegan;
            _inputManager.PanHorizontalExecuting += OnPanHorizontalExecuting;
            _inputManager.PanHorizontalEnd += OnCameraStateModifiedCompleted;
            _inputManager.PanVerticalBegin += OnPanVerticalBegan;
            _inputManager.PanVerticalExecuting += OnPanVerticalExecuting;
            _inputManager.PanVerticalEnd += OnCameraStateModifiedCompleted;
            _inputManager.ZoomBegin += OnZoomBegan;
            _inputManager.ZoomExecuting += OnZoomExecuting;
            _inputManager.ZoomEnd += OnZoomEnded;
        }

        private void UnSubscribeEvents()
        {
            _inputManager.PanHorizontalBegin -= OnPanHorizontalBegan;
            _inputManager.PanHorizontalExecuting -= OnPanHorizontalExecuting;
            _inputManager.PanHorizontalEnd -= OnCameraStateModifiedCompleted;
            _inputManager.PanVerticalBegin -= OnPanVerticalBegan;
            _inputManager.PanVerticalExecuting -= OnPanVerticalExecuting;
            _inputManager.PanVerticalEnd -= OnCameraStateModifiedCompleted;
            _inputManager.ZoomBegin -= OnZoomBegan;
            _inputManager.ZoomExecuting -= OnZoomExecuting;
            _inputManager.ZoomEnd -= OnZoomEnded;
        }

        private void RememberStartPropertyValue(CameraAnimationProperty prop)
        {
            if (!_cameraPropertyChanges.ContainsKey(prop))
            {
                _cameraPropertyChanges.Add(prop, 0);
            }

            var startValue = _cameraSystem.GetValueOf(prop);
            SetValue(prop, startValue);
        }

        private float GetValue(CameraAnimationProperty prop)
        {
            return _cameraPropertyChanges[prop];
        }

        private void SetValue(CameraAnimationProperty prop, float value)
        {
            _cameraPropertyChanges[prop] = value;
        }

        private void OnCameraStateChanged()
        {
            _cameraPropertyChanges.Clear();
            _updatingProperties.Clear();
            _cameraTemplatesManager.SaveCurrentCameraStateAsStartFrameForTemplates();
        }

        private void InitUpdating(CameraAnimationProperty prop)
        {
            _updatingProperties.Add(prop);
        }
        
        private bool IsUpdating(CameraAnimationProperty prop)
        {
            return _updatingProperties.Contains(prop);
        }

        private void StopUpdating(CameraAnimationProperty prop)
        {
            _updatingProperties.Remove(prop);
        }
    }
}