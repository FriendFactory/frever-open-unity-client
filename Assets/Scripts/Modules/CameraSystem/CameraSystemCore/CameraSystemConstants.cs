using Cinemachine;
using Common;

namespace Modules.CameraSystem.CameraSystemCore
{
    public static class CameraSystemConstants
    {
        public static class FreeLookCamera
        {
            public const float DOF_MAX = 250f;
            public const float DOF_MIN = 0f;
            public const float DOF_DEFAULT = Constants.LevelDefaults.CAMERA_DOF_DEFAULT / 1000f;
            public const float FAR_CLIP_PLANE = 200000f;
            public const float NEAR_CLIP_PLANE = 0.1f;
            public const float HEADING_BIAS = 180f;
            public const float X_AXIS_MIN = 0f;
            public const float X_AXIS_MAX = 360f;
            public const float X_AXIS_DECEL_TIME = 0f;
            public const float X_AXIS_ACCEL_TIME = 0f;
            public const float Y_AXIS_DECEL_TIME = 0f;
            public const float Y_AXIS_ACCEL_TIME = 0f;
            public static readonly string X_AXIS_INPUT_NAME = string.Empty;
            public static readonly string Y_AXIS_INPUT_NAME = string.Empty;
            public static readonly bool X_AXIS_WRAP = true;
            public static readonly CinemachineTransposer.BindingMode BINDING_MODE = CinemachineTransposer.BindingMode.WorldSpace;
        }

        public static class CameraComposer
        {
            public const float DEAD_ZONE_HEIGHT = 0.1f;
            public const float DEAD_ZONE_WIDTH = 0.2f;
            public const float SCREEN_Y = 0.35f;
            public const bool CENTER_ON_ACTIVATE = false;
        }

        public static class OrbitalTransposer
        {
            public const float X_DAMPING = 0f;
            public const float Y_DAMPING = 0f;
            public const float Z_DAMPING = 0f;
        }
    }
}
