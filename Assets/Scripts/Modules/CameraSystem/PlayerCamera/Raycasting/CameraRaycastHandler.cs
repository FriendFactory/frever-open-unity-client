using Modules.CameraSystem.CameraSettingsHandling;
using UnityEngine;
using Zenject;

namespace Modules.CameraSystem.PlayerCamera.Raycasting
{
    internal sealed class CameraRaycastHandler : ITickable
    {
        /// <summary>
        /// Buffer is used to prevent camera from moving to close to colliders which could cause camera to move inside walls and block its view. 
        /// </summary>
        private const float DISTANCE_TO_COLLIDER_BUFFER = 0.5f;
        private const float UPDATE_RADIUS_THRESHOLD = 0.1f;

        private readonly CinemachineBasedController _cinemachineBasedController;
        private readonly VerticalCameraRaycaster _upRaycaster;
        private readonly VerticalCameraRaycaster _downRaycaster;
        private readonly HorizontalCameraRaycaster _horizontalRaycaster;
        
        private bool _isRayCasting;

        private CameraSetting _cameraSetting; 

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public CameraRaycastHandler(CinemachineBasedController cinemachineBasedController)
        {
            _cinemachineBasedController = cinemachineBasedController;
            _upRaycaster = new VerticalCameraRaycaster();
            _downRaycaster = new VerticalCameraRaycaster();
            _horizontalRaycaster = new HorizontalCameraRaycaster();

            _downRaycaster.RayColor = Color.yellow;
            _downRaycaster.RayHitColor = Color.cyan;

            _cinemachineBasedController.ActivatedStatusChanged -= EnableRayCasting;
            _cinemachineBasedController.ActivatedStatusChanged += EnableRayCasting;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        public void Tick()
        {
            if (!_isRayCasting || !_cinemachineBasedController.IsActive) return;
            if(!_cinemachineBasedController.IsLookingAt || !_cinemachineBasedController.HasTarget) return;

            UpdateRaycasters();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void SetCameraSetting(CameraSetting cameraSetting)
        {
            _cameraSetting = cameraSetting;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void EnableRayCasting(bool enable)
        {
            _isRayCasting = enable;
        }

        private void UpdateRaycasters()
        {
            var cameraPos = _cinemachineBasedController.CameraPosition;
            var target = _cinemachineBasedController.Target;
            var lookAtPosition = target.position;
            var rootPosition = target.root.position;

            _upRaycaster.RootPosition = rootPosition;
            _downRaycaster.RootPosition = rootPosition;

            _horizontalRaycaster.Raycast(lookAtPosition, cameraPos);
            UpdateOrbitRadiusMax(_horizontalRaycaster.Distance);

            var positionUp = new Vector3(lookAtPosition.x, lookAtPosition.y + 1, lookAtPosition.z);
            _upRaycaster.Raycast(lookAtPosition, positionUp);
            UpdateUpperRigHeight(_upRaycaster.Distance);
        }
        
        private void UpdateUpperRigHeight(float distance)
        {
            var maxRigHeight = _upRaycaster.ColliderIsHit ? distance : _cameraSetting.OrbitRadiusMax;
            _cinemachineBasedController.MaxRigHeight = maxRigHeight;
        }

        private void UpdateOrbitRadiusMax(float radius)
        {
            if (!_horizontalRaycaster.ColliderIsHit)
            {
                var radiusDifference = Mathf.Abs(_cameraSetting.OrbitRadiusMaxDefault - _cameraSetting.OrbitRadiusMax);
                if (radiusDifference < UPDATE_RADIUS_THRESHOLD) return;
                _cameraSetting.OrbitRadiusMax = _cameraSetting.OrbitRadiusMaxDefault;
                return;
            }
            
            var newRadius = Mathf.Clamp(radius - DISTANCE_TO_COLLIDER_BUFFER, _cameraSetting.OrbitRadiusMin, _cameraSetting.OrbitRadiusMaxDefault);
            var isRadiusUpdateValid = Mathf.Abs(_cameraSetting.OrbitRadiusMax - newRadius) > UPDATE_RADIUS_THRESHOLD;
            if (!isRadiusUpdateValid) return;
            _cameraSetting.OrbitRadiusMax = newRadius;
        }
    }
}