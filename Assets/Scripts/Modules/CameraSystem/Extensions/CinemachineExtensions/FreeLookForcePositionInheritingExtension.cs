using System.Reflection;
using Cinemachine;
using UnityEngine;

namespace Modules.CameraSystem.Extensions.CinemachineExtensions
{
    /// <summary>
    ///     Allows to inherit camera game object position and rotation not only if you switch from another virtual camera(as it
    ///     works from package),
    ///     but also if you just get enabled component, or when you switch from UnityEngine.Camera.
    ///     Also it allows to calculate(force update) FreeLook camera specific properties(X Axis, Y Axis, Orbit etc)
    ///     without waiting for 1 frame(cinemachine update) based on current camera transform position/rotation.
    ///     Copied from base class logic(how it applies inherits position/rotation on camera changing), Cinemachine Package v
    /// </summary>
    internal static class FreeLookForcePositionInheritingExtension
    {
        /// <summary>
        ///     We need those values for hacking CinemachineFreeLook camera position behaviour.
        ///     By some reason during inheriting position it restore CinemachineFreeLook state(x and y axis) with slight values
        ///     deviation
        ///     Since we want to be able shift between transform values and cinemachine values recorded in txt file, we need to
        ///     have best accuracy for inheriting position
        ///     Unity Team Support recommended us to write our custom solution, instead of Cinemachine(what we can't agree since we
        ///     are in pre-release stage),
        ///     through debugging we found a way hot to "inject" suspected values to cinemachine state during inheritance
        ///     Every time we put camera based on transform values, we should update those values as well for x/y axis, since we
        ///     store both of them in txt file
        /// </summary>
        public static float OverrideXAxis = -1;
        public static float OverrideYAxis = -1;

        private static bool _initialized;

        //cached access to private members of CinemachineFreeLook type
        private static MethodInfo _updateRigCache;
        private static MethodInfo _getYAxisClosestValue;
        private static MethodInfo _pushSettingsToRigs;
        private static MethodInfo _pullStateFromVirtualCamera;
        private static FieldInfo _mOrbitalsField;
        private static FieldInfo _stateField;
        private static FieldInfo _rigsField;

        public static void InheritPositionForce(this CinemachineFreeLook freeLook, Camera targetCamera, Vector3 worldUp,
            float deltaTime = 0.001f)
        {
            if (!_initialized) Init();
            if (targetCamera == null) return;
            freeLook.PreviousStateIsValid = false;

            var cameraTransform = targetCamera.transform;
            var cameraPos = cameraTransform.position;

            UpdateRigCache(freeLook);
            UpdateAxis(freeLook, cameraPos, worldUp);

            var freeLookTransform = freeLook.transform;
            freeLookTransform.position = cameraPos;
            freeLookTransform.rotation = cameraTransform.rotation;

            var state = _pullStateFromVirtualCamera.Invoke(freeLook, new object[] {worldUp, freeLook.m_Lens});
            _stateField.SetValue(freeLook, state);
            freeLook.PreviousStateIsValid = false;
            PushSettingsToRigsInternal(freeLook);

            var rigs = _rigsField.GetValue(freeLook) as CinemachineVirtualCamera[];
            for (var i = 0; i < 3; ++i)
            {
                rigs[i].InternalUpdateCameraState(worldUp, deltaTime);
            }
            freeLook.InternalUpdateCameraState(worldUp, deltaTime);
        }

        private static void Init()
        {
            var freeLookType = typeof(CinemachineFreeLook);

            _updateRigCache = freeLookType
                .GetMethod("UpdateRigCache", BindingFlags.NonPublic | BindingFlags.Instance);

            _mOrbitalsField = freeLookType.GetField("mOrbitals", BindingFlags.Instance | BindingFlags.NonPublic);

            _getYAxisClosestValue = freeLookType
                .GetMethod("GetYAxisClosestValue", BindingFlags.NonPublic | BindingFlags.Instance);

            _pullStateFromVirtualCamera = freeLookType.GetMethod("PullStateFromVirtualCamera",
                BindingFlags.NonPublic | BindingFlags.Instance);

            _stateField = freeLookType.GetField("m_State", BindingFlags.NonPublic | BindingFlags.Instance);

            _pushSettingsToRigs = freeLookType
                .GetMethod("PushSettingsToRigs", BindingFlags.NonPublic | BindingFlags.Instance);

            _rigsField = freeLookType.GetField("m_Rigs", BindingFlags.NonPublic | BindingFlags.Instance);

            _initialized = true;
        }


        private static void UpdateRigCache(CinemachineFreeLook freeLook)
        {
            _updateRigCache?.Invoke(freeLook, null);
        }

        private static void UpdateAxis(CinemachineFreeLook freeLook, Vector3 cameraPos, Vector3 worldUp)
        {
            if (freeLook.m_BindingMode != CinemachineTransposer.BindingMode.SimpleFollowWithWorldUp)
            {
                var orbits = _mOrbitalsField.GetValue(freeLook) as CinemachineOrbitalTransposer[];
                freeLook.m_XAxis.Value = OverrideXAxis < 0
                    ? orbits[1].GetAxisClosestValue(cameraPos, worldUp)
                    : OverrideXAxis;
            }

            freeLook.m_YAxis.Value = OverrideYAxis < 0
                ? (float) _getYAxisClosestValue.Invoke(freeLook, new object[] {cameraPos, worldUp})
                : OverrideYAxis;
        }

        private static void PushSettingsToRigsInternal(CinemachineFreeLook freeLook)
        {
            _pushSettingsToRigs.Invoke(freeLook, null);
        }
    }
}