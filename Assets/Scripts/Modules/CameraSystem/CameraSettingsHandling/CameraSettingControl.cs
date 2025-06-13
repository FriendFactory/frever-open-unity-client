using Cinemachine;
using Modules.CameraSystem.CameraAnimations;
using Modules.CameraSystem.CameraSystemCore;
using Modules.CameraSystem.PlayerCamera;

namespace Modules.CameraSystem.CameraSettingsHandling
{
    internal sealed class CameraSettingControl
    {
        private const float HEIGHT_REFERENCE_VALUE = 5f;
        private const float DEFAULT_Y_START_POS = 0.3f;
        private readonly CinemachineBasedController _cinemachineBasedController;
        private readonly CinemachineFreeLook _cinemachineFreeLook;
        private readonly CameraAnimationProperty[] _startSettingProperties;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public CameraSetting CurrentlyAppliedSetting { get; private set; }
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public CameraSettingControl(CinemachineBasedController cinemachineBasedController, CinemachineFreeLook cinemachineFreeLook)
        {
            _cinemachineBasedController = cinemachineBasedController;
            _cinemachineFreeLook = cinemachineFreeLook;
            
            _startSettingProperties = new[]
            {
                CameraAnimationProperty.OrbitRadius,
                CameraAnimationProperty.DepthOfField,
                CameraAnimationProperty.FieldOfView,
                CameraAnimationProperty.Dutch,
                CameraAnimationProperty.AxisX
            };
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void ApplySetting(CameraSetting cameraSetting)
        {
            CurrentlyAppliedSetting = cameraSetting;
            SetupDefaultCameraSettings();
            UpdateCameraStartSettings();
        }
        
        public void SetupCameraStartSettings()
        {
            _cinemachineBasedController.MaxRigHeight = CurrentlyAppliedSetting.OrbitRadiusMax;
            
            foreach (var property in _startSettingProperties)
            {
                var startValue = CurrentlyAppliedSetting.GetStartValueWithProperty(property);
                _cinemachineBasedController.Set(property, startValue);
            }
            
            SetupStartYPosition();
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void SetupStartYPosition()
        {
            var distancePercentage = HEIGHT_REFERENCE_VALUE / CurrentlyAppliedSetting.OrbiRadiusStart;
            var yPos = distancePercentage * DEFAULT_Y_START_POS;
            _cinemachineBasedController.Set(CameraAnimationProperty.AxisY, yPos);
        }
        
        private void SetupDefaultCameraSettings()
        {
            SetupDefaultRigsSettings();
            _cinemachineFreeLook.m_XAxis.m_MinValue = CameraSystemConstants.FreeLookCamera.X_AXIS_MIN;
            _cinemachineFreeLook.m_XAxis.m_MaxValue = CameraSystemConstants.FreeLookCamera.X_AXIS_MAX;
            _cinemachineFreeLook.m_YAxis.m_DecelTime = CameraSystemConstants.FreeLookCamera.Y_AXIS_DECEL_TIME;
            _cinemachineFreeLook.m_YAxis.m_AccelTime = CameraSystemConstants.FreeLookCamera.Y_AXIS_ACCEL_TIME;
            _cinemachineFreeLook.m_XAxis.m_DecelTime = CameraSystemConstants.FreeLookCamera.X_AXIS_DECEL_TIME;
            _cinemachineFreeLook.m_XAxis.m_AccelTime = CameraSystemConstants.FreeLookCamera.X_AXIS_ACCEL_TIME;
            _cinemachineFreeLook.m_BindingMode = CameraSystemConstants.FreeLookCamera.BINDING_MODE;
            _cinemachineFreeLook.m_XAxis.m_InputAxisName = CameraSystemConstants.FreeLookCamera.X_AXIS_INPUT_NAME;
            _cinemachineFreeLook.m_YAxis.m_InputAxisName = CameraSystemConstants.FreeLookCamera.Y_AXIS_INPUT_NAME;
            _cinemachineFreeLook.m_Lens.NearClipPlane = CameraSystemConstants.FreeLookCamera.NEAR_CLIP_PLANE;
            _cinemachineFreeLook.m_Lens.FarClipPlane = CameraSystemConstants.FreeLookCamera.FAR_CLIP_PLANE;
            _cinemachineFreeLook.m_XAxis.m_Wrap = CameraSystemConstants.FreeLookCamera.X_AXIS_WRAP;
        }
        
        private void SetupDefaultRigsSettings()
        {
            var rigs = new[] {_cinemachineFreeLook.GetRig(0), _cinemachineFreeLook.GetRig(1), _cinemachineFreeLook.GetRig(2)};

            for (int i = 0; i < rigs.Length; i++)
            {
                var composer = rigs[i].GetCinemachineComponent<CinemachineComposer>();
                composer.m_DeadZoneHeight = CameraSystemConstants.CameraComposer.DEAD_ZONE_HEIGHT;
                composer.m_DeadZoneWidth = CameraSystemConstants.CameraComposer.DEAD_ZONE_WIDTH;
                if(i != 0) composer.m_ScreenY = CameraSystemConstants.CameraComposer.SCREEN_Y;
                composer.m_CenterOnActivate = CameraSystemConstants.CameraComposer.CENTER_ON_ACTIVATE;
                
                var orbitalTransposer = rigs[i].GetCinemachineComponent<CinemachineOrbitalTransposer>();
                orbitalTransposer.m_XDamping = CameraSystemConstants.OrbitalTransposer.X_DAMPING;
                orbitalTransposer.m_YDamping = CameraSystemConstants.OrbitalTransposer.Y_DAMPING;
                orbitalTransposer.m_ZDamping = CameraSystemConstants.OrbitalTransposer.Z_DAMPING;
            }
        }
        
        private void UpdateCameraStartSettings()
        {
            if (IsNextMaxDistanceLowerThanCurrent())
            {
                _cinemachineBasedController.MaxRigHeight = CurrentlyAppliedSetting.OrbitRadiusMax;
                _cinemachineBasedController.Set(CameraAnimationProperty.OrbitRadius, CurrentlyAppliedSetting.OrbitRadiusMax);
            }

            if (IsNextMaxFovLowerThanCurrent())
            {
                _cinemachineBasedController.Set(CameraAnimationProperty.FieldOfView, CurrentlyAppliedSetting.FoVStart);
            }
        }
        
        private bool IsNextMaxDistanceLowerThanCurrent()
        {
            return CurrentlyAppliedSetting.OrbitRadiusMax < _cinemachineBasedController.GetValue(CameraAnimationProperty.OrbitRadius);
        }

        private bool IsNextMaxFovLowerThanCurrent()
        {
            return CurrentlyAppliedSetting.FoVMax < _cinemachineBasedController.GetValue(CameraAnimationProperty.FieldOfView);
        }
    }
}
