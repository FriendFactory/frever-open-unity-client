using System;
using Extensions;
using JetBrains.Annotations;
using Modules.CameraSystem.CameraAnimations;
using Modules.CameraSystem.CameraSystemCore;
using Modules.CameraSystem.Extensions.CinemachineExtensions;
using Modules.CameraSystem.PlayerCamera;
using Modules.CameraSystem.PlayerCamera.Handlers;
using UnityEngine;
using Zenject;

namespace Modules.CameraSystem.Preview
{
    /// <summary>
    ///     Controls camera during animation preview
    ///     Important! It should be placed after all scripts which change camera state in Execution Order list to be able
    ///     properly update settings in LateUpdate
    /// </summary>
    [UsedImplicitly]
    internal sealed class TransformBasedController : ICameraController, ILateTickable
    {
        private readonly CameraDofManager _cameraDofManager;
        private readonly CinemachineBasedController _cinemachineBasedController;

        private Transform _anchor;
        private Camera _camera;
        private Vector3 _relativePosition;
        private Vector3 _relativeRotation;
        private bool _enabled;

        private bool _hasAnchor;
        private bool _hasCamera;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public AnimationType TargetAnimationType => AnimationType.TransformBased;
        public bool IsActive => _enabled;

        private Transform CameraTransform => _camera?.transform;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action AppliedStateToGameObject;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public TransformBasedController(CameraDofManager cameraDofManager, CinemachineBasedController cinemachineBasedController)
        {
            _cameraDofManager = cameraDofManager;
            _cinemachineBasedController = cinemachineBasedController;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void SetAnchor(Transform anchor)
        {
            UnSubscribeFromCurrentAnchor();
        
            _anchor = anchor;
            _hasAnchor = anchor != null;

            SubscribeToCurrentAnchor();
        }

        public void Enable(bool activate)
        {
            _enabled = activate;
        }
        
        public void ForceUpdate()
        {
            UpdateState();
        }

        public void SetCamera(Camera cameraComponent)
        {
            UnsubscribeFromCurrentCamera();
            
            _camera = cameraComponent;
            _hasCamera = _camera != null;
            
            SubscribeToCurrentCamera();
        }

        public void Set(CameraAnimationProperty property, float value)
        {
            switch (property)
            {
                case CameraAnimationProperty.PositionX:
                    SetPositionX(value);
                    break;
                case CameraAnimationProperty.PositionY:
                    SetPositionY(value);
                    break;
                case CameraAnimationProperty.PositionZ:
                    SetPositionZ(value);
                    break;
                case CameraAnimationProperty.RotationX:
                    SetRotationX(value);
                    break;
                case CameraAnimationProperty.RotationY:
                    SetRotationY(value);
                    break;
                case CameraAnimationProperty.RotationZ:
                    SetRotationZ(value);
                    break;
                case CameraAnimationProperty.DepthOfField:
                    SetDepthOfField(value);
                    break;
                case CameraAnimationProperty.FieldOfView:
                    SetFieldOfView(value);
                    break;
                case CameraAnimationProperty.FocusDistance:
                    SetFocusDistance(value);
                    break;
                default:
                    SyncCinemachineStateWithTransform(property, value);
                    break;
            }
        }
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        public void LateTick()
        {
            if (!_enabled) return;
            UpdateState();
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void UpdateState()
        {
            if(!_hasAnchor || !_hasCamera) return;
            
            UpdatePosition(); 
            UpdateRotation();
            AppliedStateToGameObject?.Invoke();
        }

        private void UpdateRotation()
        {
            var localQ = Quaternion.Euler(_relativeRotation);
            var worldQ = _anchor.rotation * localQ;
            CameraTransform.rotation = worldQ;
        }

        private void UpdatePosition()
        {
            CameraTransform.position = ConvertToWorldSpace(_relativePosition);
        }

        private Vector3 ConvertToWorldSpace(Vector3 relativePos)
        {
            return _anchor.TransformPoint(relativePos);
        }

        private void SetDepthOfField(float value)
        {
            _cameraDofManager.SwitchDoF(true);
            _cameraDofManager.SetDepthOfField(value);
        }
        
        private void SetPositionX(float value)
        {
            _relativePosition.x = value;
        }

        private void SetPositionY(float value)
        {
            _relativePosition.y = value;
        }

        private void SetPositionZ(float value)
        {
            _relativePosition.z = value;
        }

        private void SetRotationX(float value)
        {
            _relativeRotation.x = value;
        }

        private void SetRotationY(float value)
        {
            _relativeRotation.y = value;
        }

        private void SetRotationZ(float value)
        {
            _relativeRotation.z = value;
        }

        private void SetFieldOfView(float fov)
        {
            _camera.fieldOfView = fov;
        }

        private void SetFocusDistance(float value)
        {
            _cameraDofManager.SetFocusDistance(value);
        }

        private void OnAnchorDestroyed()
        {
            _hasAnchor = false;
            _anchor = null;
        }

        private void UnSubscribeFromCurrentAnchor()
        {
            if (!_hasAnchor) return;
            _anchor.RemoveListenerFromDestroyEvent(OnAnchorDestroyed);
        }
        
        private void SubscribeToCurrentAnchor()
        {
            if (!_hasAnchor) return;
            _anchor.AddListenerToDestroyEvent(OnAnchorDestroyed);
        }
        
        private void OnCameraDestroyed()
        {
            _hasCamera = false;
            _camera = null;
        }
        
        private void SubscribeToCurrentCamera()
        {
            if (!_hasCamera) return;
            CameraTransform.AddListenerToDestroyEvent(OnCameraDestroyed);
        }

        private void UnsubscribeFromCurrentCamera()
        {
            if (!_hasCamera) return;
            CameraTransform.RemoveListenerFromDestroyEvent(OnCameraDestroyed);
        }

        //Workaround for [FREV-6222]. Seems it's worth to set CinemachineFreeLook values even on disabled component,
        //to have proper transform inheritance logic
        private void SyncCinemachineStateWithTransform(CameraAnimationProperty prop, float value)
        {
            switch (prop)
            {
                case CameraAnimationProperty.AxisX:
                    FreeLookForcePositionInheritingExtension.OverrideXAxis = value; 
                    _cinemachineBasedController.Set(prop, value);
                    break;
                case CameraAnimationProperty.AxisY:
                    FreeLookForcePositionInheritingExtension.OverrideYAxis = value;
                    _cinemachineBasedController.Set(prop, value);
                    break;
                default:
                    _cinemachineBasedController.Set(prop, value);
                    break;
            }
        }
    }
}