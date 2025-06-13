using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cinemachine;
using Common.TimeManaging;
using Modules.CameraSystem.CameraAnimations;
using Modules.CameraSystem.CameraAnimations.Template;
using Modules.CameraSystem.CameraSettingsHandling;
using UnityEngine;

namespace Modules.CameraSystem.CameraSystemCore
{
    public interface ICameraSettingControl
    {
        event Action CameraSettingsChanged;
        void ChangeCameraSetting(CameraSetting cameraSetting);
        CameraSetting GetCurrentCameraSetting();
        void SetupCameraSettingsStartValues();
    }

    public interface INoiseProfileControl
    {
        event Action<int> NoiseProfileChanged;
        void SetNoiseProfile(int value);
        int GetNoiseProfileId();
        NoiseSettings[] GetNoiseProfiles();
    }

    public interface ICameraPlayer
    {
        void SetPlaybackSpeed(float speed);
        
        void PlayAnimation(RecordedCameraAnimationClip target);
        void PlayTemplate(TemplateCameraAnimationClip clip, TemplatePlayingMode mode);
        void StopAnimation(bool returnToStartPosition = true);
        void PauseAnimation();
        void ResumeAnimation();

        /// <summary>
        /// Put camera to frame state
        /// </summary>
        void Simulate(CameraAnimationFrame frame);

        /// <summary>
        /// Allows to simulate any clip(either recorded by user or template) for any time moment
        /// For templates it starts running it from template's target start position
        /// </summary>
        void Simulate(CameraAnimationClip clip, float time);

        /// <summary>
        /// Allows to simulate template for any time moment with extra argument for controlling forced put
        /// template on target start position. It might be used for saving performance purpose, since just Simulate
        /// force set Template from target start position
        /// </summary>
        void SimulateTemplate(TemplateCameraAnimationClip clip, float time, bool forceStartFromTargetStartPosition);
        
        /// <summary>
        /// Camera system (cinemachine) does not allow multi times updating camera state per frame.
        /// If user invokes Simulate() api for it will use data from last time simulation and apply at the end of frame.
        /// Use this method for being able to recalculate/apply few states at the single frame(for instance, for camera path prediction).
        /// Important! Don't forget disable when it is not needed anymore
        /// </summary>
        void AllowCameraForMultiTimeSimulationInSingleFrame(bool isOn);
        
        void SetCameraOnStartPosition(CameraAnimationClip clip);
        void SetCameraOnEndPosition(CameraAnimationClip clip);
    }

    public interface ICameraRecorder
    {
        void StartRecording(ITimeSource timeSource);
        CameraAnimationSavingResult StopRecording(bool useUniqueName = false);
        void CancelRecording();
    }

    public interface ICameraControl
    {
        event Action CameraUpdated;
        CinemachineComposer CinemachineComposer { get; }
        void EnableCameraRendering(bool enable);
        void SetCameraComponents(Camera camera, CinemachineBrain brain);
        
        /// <summary>
        /// Used to put offset on the camera in relation to the forward direction.
        /// </summary>
        void SetHeadingBias(float value);
        void ForgetCamera();
        void SetCameraAnchor(Transform anchor);
        void ForgetLookAtTarget();
        void SetTargets(GameObject lookAt, GameObject follow, bool recenter = true);
        void SetFocusPointAdjustmentCurve(AnimationCurve animationCurve);
        void SetGroupTargets(bool recenter = true);
        void SetLookAtGroupMembers(IEnumerable<Transform> members);
        void SetFollowGroupMembers(IEnumerable<Transform> members);
        void SetLookAt(bool value);
        void SetFollow(bool value);
        void UpdateFollowingState();
        void RecenterFocus();
        void SetFocusDistance(float value);
        void Set(CameraAnimationProperty property, float value);
        void SetTweened(CameraAnimationProperty property, float value, float time, Action onComplete = null);
        float GetValueOf(CameraAnimationProperty property);
        Vector2 GetMinMaxValuesOf(CameraAnimationProperty property);
        CinemachineTargetGroup GetCinemachineLookAtTargetGroup();
        GameObject GetLookAtTargetGroupGameObject();
        CinemachineTargetGroup GetCinemachineFollowTargetGroup();
        GameObject GetFollowTargetGroupGameObject();
        void PutCinemachineToState(CameraAnimationFrame state);
        CameraAnimationFrame GetCurrentTransformBasedCameraState();
        CameraAnimationFrame GetCameraState();
        float GetXAxisValueToFocusOnPoint(Transform point);
        
        bool Follow { get; set; }
        bool LookAt { get; set; }
        int LayerMask { get; set; }
    }

    public interface ICameraAssetFileEditor
    {
        CameraAnimationSavingResult ReplaceProperties(RecordedCameraAnimationClip originAnimation, string sourceForReplacingValuesAnimationString, CameraAnimationProperty[] propertiesToReplace);
    }

    public interface ICameraSystem : ICameraSettingControl, INoiseProfileControl, ICameraPlayer, ICameraRecorder, ICameraControl, ICameraAssetFileEditor
    {
        event Action<bool> EnabledStatusChanged;

        void Initialize();
        void CleanUp();
        void Enable(bool isOn);
        Task ClearCachedFilesAsync();
        void ForceUpdate();
        /// <summary>
        /// Set camera on position to be focused on target even though both lookAt and follow settings are disabled
        /// </summary>
        void FocusOnTargetForce();

        void EnableCollider(bool p0);
    }
}