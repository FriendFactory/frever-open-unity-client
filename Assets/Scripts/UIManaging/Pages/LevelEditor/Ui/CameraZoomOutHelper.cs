using System.Collections;
using Common;
using Modules.CameraSystem.CameraAnimations;
using Modules.CameraSystem.CameraSystemCore;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui
{
    public class CameraZoomOutHelper
    {
        private Coroutine _zoomOutRoutine;
        private float _zoomOutIncrement;
        private float _zoomOutTime = 2f;
        private float _timeElapsed;
        private float _originFov;
        private float _originDistance;
    
        private readonly ICameraSystem _cameraSystem;

        public CameraZoomOutHelper(ICameraSystem cameraSystem)
        {
            _cameraSystem = cameraSystem;
        }
        
        public void ZoomOut()
        {
            if (_zoomOutRoutine != null)
            {
                CoroutineSource.Instance.StopCoroutine(_zoomOutRoutine);
                _zoomOutRoutine = null;
            }

            var targetFov = _cameraSystem.GetCurrentCameraSetting().FoVStart;
            var currentFov = _cameraSystem.GetValueOf(CameraAnimationProperty.FieldOfView);
            var targetDistance = _cameraSystem.GetCurrentCameraSetting().OrbiRadiusStart;
            var currentDistance = _cameraSystem.GetValueOf(CameraAnimationProperty.OrbitRadius);
            
            _originFov = currentFov;
            _originDistance = currentDistance;

            if (currentDistance >= targetDistance) return;
            
            _zoomOutIncrement = 0;
            _timeElapsed = 0;
            _zoomOutRoutine = CoroutineSource.Instance.StartCoroutine(ZoomOut(currentFov, targetFov, currentDistance, targetDistance));
        }

        public void ReturnToOriginalZoom()
        {
            if (_zoomOutRoutine != null)
            {
                CoroutineSource.Instance.StopCoroutine(_zoomOutRoutine);
                _zoomOutRoutine = null;
            }
            
            _cameraSystem.Set(CameraAnimationProperty.FieldOfView,_originFov);
            _cameraSystem.Set( CameraAnimationProperty.OrbitRadius,_originDistance);
        }
    
        private IEnumerator ZoomOut(float intialFov, float targetFov, float initialDistance, float targetDistance)
        {
            while (!IsTargetFovReached(targetFov) || !IsTargetDistanceReached(targetDistance))
            {
                _zoomOutIncrement += _timeElapsed / _zoomOutTime;
                var fov = Mathf.Lerp(intialFov, targetFov, _zoomOutIncrement);
                var distance = Mathf.Lerp(initialDistance, targetDistance, _zoomOutIncrement);
                _cameraSystem.Set(CameraAnimationProperty.FieldOfView,fov);
                _cameraSystem.Set(CameraAnimationProperty.OrbitRadius,distance);
                _timeElapsed += Time.deltaTime;
                yield return null;
            }

            _cameraSystem.Set(CameraAnimationProperty.FieldOfView, targetFov);
            _cameraSystem.Set(CameraAnimationProperty.OrbitRadius,targetDistance);
            _zoomOutRoutine = null;
        }

        private bool IsTargetFovReached(float targetFov)
        {
            return Mathf.Abs(_cameraSystem.GetValueOf(CameraAnimationProperty.FieldOfView) - targetFov) < 0.1f;
        }
        private bool IsTargetDistanceReached(float targetDistance)
        {
            return Mathf.Abs(_cameraSystem.GetValueOf(CameraAnimationProperty.OrbitRadius) - targetDistance) < 0.1f;
        }
    }
}
