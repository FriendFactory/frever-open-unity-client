using System;
using JetBrains.Annotations;
using Modules.CameraSystem.CameraSystemCore;
using Modules.CameraSystem.PlayerCamera;
using Modules.CameraSystem.PlayerCamera.Handlers;
using Zenject;

namespace Modules.CameraSystem.Extensions.CinemachineExtensions
{
    /// <summary>
    /// Set DoF in runtime based on distance to target only during camera controlling with Cinemachine camera(templates playing/recording/user manual moving).
    /// It should be disabled when we play user recorded animation, where we have all key frames for DoF
    /// </summary>
    [UsedImplicitly]
    internal sealed class CameraDofUpdater 
    {
        private const float DOF_MIN_DISABLE_PRECISION = 0.05f;
        private const float DOF_MIN_DISABLE_DISTANCE = 1.0f;
        private const float DOF_MAX_DISABLE_DISTANCE = 56f;

        private const float RANGE_BETWEEN_MAX_AND_DEFAULT_DOF = CameraSystemConstants.FreeLookCamera.DOF_MAX - CameraSystemConstants.FreeLookCamera.DOF_DEFAULT;
        private const float DOF_INCREMENT_PER_DISTANCE_ABOVE_MIN_DISTANCE = RANGE_BETWEEN_MAX_AND_DEFAULT_DOF / (DOF_MAX_DISABLE_DISTANCE - DOF_MIN_DISABLE_DISTANCE);

        private float _previousDistance;
        private CinemachineBasedController _cinemachineBasedController;
        private CameraDofManager _dofManager;
        private bool _enabled;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        [Inject]
        private void Construct(CameraDofManager dofManager, CinemachineBasedController cinemachineBasedController)
        {
            _dofManager = dofManager;
            _cinemachineBasedController = cinemachineBasedController;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Enable(bool value)
        {
            if(_enabled == value) return;
            _enabled = value;

            if (value) 
                _cinemachineBasedController.AppliedStateToGameObject += RecalculateDofAndFocalDistance;
            else
                _cinemachineBasedController.AppliedStateToGameObject -= RecalculateDofAndFocalDistance;
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void RecalculateDofAndFocalDistance()
        {
            UpdateDepthOfFieldSettings();
            UpdateFocusDistanceSettings();
        }
        
        private void SetDepthOfField(float value)
        {
           _dofManager.SetDepthOfField(value);
        }

        private void SetFocusDistance(float value)
        {
           _dofManager.SetFocusDistance(value);
        }

        private void UpdateDepthOfFieldSettings()
        {
            var currentDistance = _cinemachineBasedController.GetRigRadius();

            if (currentDistance <= DOF_MIN_DISABLE_DISTANCE && _previousDistance > DOF_MIN_DISABLE_DISTANCE)
                SetDepthOfField(CameraSystemConstants.FreeLookCamera.DOF_DEFAULT);

            UpdateDofSettingsStatus(currentDistance);
        }

        private void UpdateFocusDistanceSettings()
        {
            if(!_cinemachineBasedController.HasTarget) return;
            var distanceToTarget = _cinemachineBasedController.DistanceToTarget();
            SetFocusDistance(distanceToTarget);
        }

        private bool ShouldDepthOfFieldBeEnabled(float currentDistance)
        {
            if (currentDistance <= DOF_MIN_DISABLE_DISTANCE) 
                return true;
            
            var isWithinMinDisablingRange  = Math.Abs(currentDistance - DOF_MIN_DISABLE_DISTANCE) < DOF_MIN_DISABLE_PRECISION;
            
            if (currentDistance > DOF_MAX_DISABLE_DISTANCE || isWithinMinDisablingRange) 
                return false;
            
            return CanDofRemainActiveAtDistance(currentDistance);
        }

        private void UpdateDofSettingsStatus(float currentDistance)
        {
            var shouldBeEnabled = ShouldDepthOfFieldBeEnabled(currentDistance);
            _previousDistance = currentDistance;
            _dofManager.SwitchDoF(shouldBeEnabled);
        }

        private bool CanDofRemainActiveAtDistance(float distance)
        {
            var distanceAboveMinDisableThreshold = distance - DOF_MIN_DISABLE_DISTANCE;
            var extraDofAmountRequired = distanceAboveMinDisableThreshold * DOF_INCREMENT_PER_DISTANCE_ABOVE_MIN_DISTANCE;
            var minDofRequiredToRemainActive = extraDofAmountRequired + CameraSystemConstants.FreeLookCamera.DOF_DEFAULT;
            return _dofManager.GetDepthOfField() >= minDofRequiredToRemainActive;
        }
    }
}