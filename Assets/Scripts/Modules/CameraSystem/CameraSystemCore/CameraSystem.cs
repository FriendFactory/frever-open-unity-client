using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Cinemachine;
using Common;
using Common.ApplicationCore;
using Common.TimeManaging;
using DG.Tweening;
using Extensions;
using JetBrains.Annotations;
using Modules.CameraSystem.CameraAnimations;
using Modules.CameraSystem.CameraAnimations.Template;
using Modules.CameraSystem.CameraSettingsHandling;
using Modules.CameraSystem.Extensions;
using Modules.CameraSystem.PlayerCamera;
using Modules.CameraSystem.PlayerCamera.Handlers;
using Modules.CameraSystem.PlayerCamera.Raycasting;
using Modules.CameraSystem.Preview;
using UnityEngine;

namespace Modules.CameraSystem.CameraSystemCore
{
    [UsedImplicitly]
    internal sealed class CameraSystem : ICameraSystem
    {
        private readonly AnimationPlayingManager _animationPlayingManager;
        private readonly CameraAnimationSaver _animationSaver;
        private readonly CameraAnimator _animator;
        private readonly CameraDofManager _cameraDofManager;
        private readonly CameraAnimationPropsReplacer _cameraAnimationPropsReplacer;

        private readonly ICameraController[] _cameraControllers;
   
        private readonly CameraAnimationRecorder _cameraRecorder;
        private readonly ICameraTemplatesManager _cameraTemplatesManager;

        private readonly CinemachineBasedController _cinemachineBasedController;
        private readonly TransformBasedController _transformBasedController;
        private readonly CameraSettingControl _cameraSettingControl;
        private readonly CameraRaycastHandler _cameraRaycastHandler;
        private Camera _targetCamera;
        private Transform _cameraAnchor;
        
        private bool _isEnabled;
        private bool _isInitialized;
        private bool _multiTimePerFrameUpdatingEnabled;
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public CameraSystem(IBridge bridge, CinemachineBasedController cinemachineBasedController,
            TransformBasedController transformBasedController, CameraAnimationSaver animationSaver, CameraAnimationRecorder cameraRecorder, 
            CameraAnimator animator, AnimationPlayingManager animationPlayingManager, CameraSettingControl cameraSettingControl,
            CameraRaycastHandler cameraRaycastHandler, ICameraTemplatesManager cameraTemplatesManager, IAppEventsSource appEventsSource, CameraDofManager cameraDofManager, CameraAnimationPropsReplacer cameraAnimationPropsReplacer)
        {
            _cinemachineBasedController = cinemachineBasedController;
            _transformBasedController = transformBasedController;
            _animationSaver = animationSaver;
            _cameraRecorder = cameraRecorder;
            _animator = animator;
            _animationPlayingManager = animationPlayingManager;
            _cameraSettingControl = cameraSettingControl;
            _cameraRaycastHandler = cameraRaycastHandler;
            _cameraControllers = new ICameraController[] {_cinemachineBasedController, _transformBasedController};
            _cameraTemplatesManager = cameraTemplatesManager;
            _cameraDofManager = cameraDofManager;
            _cameraAnimationPropsReplacer = cameraAnimationPropsReplacer;
            appEventsSource.ApplicationQuit += OnApplicationQuit;
        }

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public CinemachineComposer CinemachineComposer => _cinemachineBasedController.GetCinemachineComposer();
        
        
        public bool LookAt       
        {
            get => _cinemachineBasedController.IsLookingAt;
            set => SetLookAt(value);
        }

        public bool Follow
        {
            get => _cinemachineBasedController.IsFollowing;
            set => SetFollow(value);
        }
        
        public int LayerMask
        {
            get => _targetCamera.cullingMask;
            set => _targetCamera.cullingMask = value; 
        }

        private bool CinemachineEnabled => _cinemachineBasedController.IsActive;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action CameraSettingsChanged;
        public event Action CameraUpdated;
        public event Action<int> NoiseProfileChanged;
        public event Action<bool> EnabledStatusChanged;
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Initialize()
        {
            if (_isInitialized)
                return;
            
            _cinemachineBasedController.NoiseProfileChanged += OnNoiseProfileChanged;
            
            foreach (var cameraController in _cameraControllers)
            {
                cameraController.AppliedStateToGameObject += OnCameraUpdated;
            }

            _isInitialized = true;
        }

        public void EnableCameraRendering(bool enable)
        {
            if (_targetCamera == null)
            {
                throw new InvalidOperationException("Can't enable/disable camera rendering. Target camera has not been setup.");
            }
            
            _targetCamera.enabled = enable;
        }

        public void ChangeCameraSetting(CameraSetting cameraSetting)
        {
            if (cameraSetting?.Id == _cameraSettingControl.CurrentlyAppliedSetting?.Id) return;
            _cameraSettingControl.ApplySetting(cameraSetting);
            _cameraRaycastHandler.SetCameraSetting(cameraSetting);
            CameraSettingsChanged?.Invoke();
        }

        public void SetCameraComponents(Camera camera, CinemachineBrain brain)
        {
            UnsubscribeFromTargetCameraDestroyEvent();
            _targetCamera = camera;
            SubscribeToTargetCameraDestroyEvent();
            
            PrepareTargetCamera(camera);
            SetupControllers(camera, brain);
        }

        public void SetHeadingBias(float value)
        {
            _cinemachineBasedController.SetHeadingBias(value);
        }

        public void ForgetCamera()
        {
            SetCameraComponents(null, null);
        }

        public void SetCameraAnchor(Transform anchor)
        {
            foreach (var cameraController in _cameraControllers)
            {
                cameraController.SetAnchor(anchor);
            }

            _cameraAnchor = anchor;
        }

        public void CleanUp()
        {
            Enable(false);
            ForgetCamera();
            ForgetAnchor();
            _cinemachineBasedController.ForgetTemporaryTarget();
            _cameraDofManager.SwitchDoF(false);
        }
        
        public void SetPlaybackSpeed(float speed)
        {
            _animator.SetSpeed(speed);
        }

        public void PlayAnimation(RecordedCameraAnimationClip target)
        {
            EnableIfNotEnabled();
            
            _animationPlayingManager.PlayAnimation(target);
        }

        public void PlayTemplate(TemplateCameraAnimationClip clip, TemplatePlayingMode mode)
        {
            EnableIfNotEnabled();
            _cameraTemplatesManager.UpdateStartPositionForTemplate(clip);
            _animationPlayingManager.PlayTemplateAnimation(clip, mode);
        }

        public void StopAnimation(bool returnToStartPosition = true)
        {
            _animationPlayingManager.StopAnimation(returnToStartPosition);
        }

        public void PauseAnimation()
        {
            _animationPlayingManager.PauseAnimation();
        }

        public void ResumeAnimation()
        {
            _animationPlayingManager.ResumeAnimation();
        }

        public void StartRecording(ITimeSource timeSource)
        {
            _cameraRecorder.StartRecording(timeSource, _cinemachineBasedController);
        }

        public CameraAnimationSavingResult StopRecording(bool useUniqueName = false)
        {
            return _cameraRecorder.StopRecording(useUniqueName);
        }

        public void CancelRecording()
        {
            _cameraRecorder.CancelRecording();
        }
        
        public CameraAnimationSavingResult ReplaceProperties(RecordedCameraAnimationClip originAnimation,
            string sourceForReplacingValuesAnimationString, CameraAnimationProperty[] propertiesToReplace)
        {
            return _cameraAnimationPropsReplacer.ReplaceProperties(originAnimation,
                                                                   sourceForReplacingValuesAnimationString,
                                                                   propertiesToReplace);
        }

        public void SetCameraOnStartPosition(CameraAnimationClip clip)
        {
            EnableIfNotEnabled();
            _animationPlayingManager.SetCameraOnStartPosition(clip);
        }

        public void SetCameraOnEndPosition(CameraAnimationClip clip)
        {
            EnableIfNotEnabled();
            _animationPlayingManager.SetCameraOnEndPosition(clip);
        }

        public void Simulate(CameraAnimationFrame frame)
        {
            EnableIfNotEnabled();
            _animationPlayingManager.Simulate(frame);
        }
        
        public void Simulate(CameraAnimationClip clip, float time)
        {
            EnableIfNotEnabled();
            _cameraTemplatesManager.PrepareClipForSimulationFrame(clip);
            AllowImmediateStateRecalculationIfEnabled();
            _animationPlayingManager.Simulate(clip, time);
        }

        public void SimulateTemplate(TemplateCameraAnimationClip clip, float time, bool forceStartFromTargetStartPosition)
        {
            SwitchToCinemachineControl();
            EnableIfNotEnabled();
            if (forceStartFromTargetStartPosition)
            {
                _cameraTemplatesManager.PrepareClipForSimulationFrame(clip);
            }

            AllowImmediateStateRecalculationIfEnabled();
            _animationPlayingManager.SimulateTemplate(clip, time);
        }

        public void SetupCameraSettingsStartValues()
        {
            _cameraSettingControl.SetupCameraStartSettings();
        }

        public void SetTargets(GameObject lookAt, GameObject follow, bool recenter = true)
        {
            SwitchToCinemachineControl();
            SetTargetsToCinemachine(lookAt, follow, recenter);
        }

        public void SetFocusPointAdjustmentCurve(AnimationCurve animationCurve)
        {
            _cinemachineBasedController.SetFocusAdjustmentCurve(animationCurve);
        }

        public void ForgetLookAtTarget()
        {
            SetTargetsToCinemachine(null, null);
        }

        public void SetLookAtGroupMembers(IEnumerable<Transform> members)
        {
            SwitchToCinemachineControl();
            _cinemachineBasedController.SetLookAtGroupMembers(members);
        }

        public void SetFollowGroupMembers(IEnumerable<Transform> members)
        {
            SwitchToCinemachineControl();
            _cinemachineBasedController.SetFollowGroupMembers(members);
        }

        public void SetGroupTargets(bool recenter = true)
        {
            var lookAt = GetCinemachineLookAtTargetGroup().gameObject;
            var follow = GetCinemachineFollowTargetGroup().gameObject;
            SetTargetsToCinemachine(lookAt, follow, recenter);
        }

        public void SetLookAt(bool value)
        {
            _cinemachineBasedController.SetLookAt(value);
        }
        
        public bool GetFollow()
        {
            return _cinemachineBasedController.IsFollowing;
        }

        public void SetFollow(bool value)
        {
            _cinemachineBasedController.SetFollow(value);
        }

        public void UpdateFollowingState()
        {
            _cinemachineBasedController.UpdateFollowingState();
        }

        public void RecenterFocus()
        {
            _cinemachineBasedController.RecenterFocus();
        }

        public void Enable(bool isOn)
        {
            if (isOn == _isEnabled) return;
            _isEnabled = isOn;
            _animator.Enable(isOn);
            if(!isOn) StopControlCamera();
            EnabledStatusChanged?.Invoke(isOn);
        }

        public void SetFocusDistance(float value)
        {
            _cinemachineBasedController.SetFocusDistance(value);
        }

        public void SetNoiseProfile(int value)
        {
            _cinemachineBasedController.SetNoiseProfile(value);
        }

        public int GetNoiseProfileId()
        {
            return _cinemachineBasedController.CurrentNoiseProfileId;
        }

        public NoiseSettings[] GetNoiseProfiles()
        {
            return _cinemachineBasedController.NoiseProfiles;
        }

        public CameraSetting GetCurrentCameraSetting()
        {
            return _cameraSettingControl.CurrentlyAppliedSetting;
        }

        public void Set(CameraAnimationProperty property, float value)
        {
            SwitchToCinemachineControl();
           
            _cinemachineBasedController.Set(property, value);
        }

        private Dictionary<CameraAnimationProperty, Coroutine> _tweenDict = new();
        
        public void SetTweened(CameraAnimationProperty property, float value, float time, Action onComplete = null)
        {
            if (_tweenDict.TryGetValue(property, out var coroutine))
            {
                CoroutineSource.Instance.SafeStopCoroutine(coroutine);
                _tweenDict.Remove(property);
            }
            
            SwitchToCinemachineControl();

            var tweenStartValue = _cinemachineBasedController.GetValue(property);
            
            var interpolateLoop = false;
            var endValue = value;
            
            if (property is CameraAnimationProperty.AxisX)
            {
                var minMax = _cinemachineBasedController.GetMinMaxValues(property);
                var absDelta = Mathf.Abs(tweenStartValue - value);
                var range = Mathf.Abs(minMax.y - minMax.x);
                var halfRange = range / 2f;
                if (absDelta > halfRange)
                {
                    interpolateLoop = true;
                    endValue = value > halfRange 
                        ? minMax.x - (range - value) 
                        : minMax.y + value;
                }
            }
            
            _tweenDict[property] = CoroutineSource.Instance.StartCoroutine(Tween());
            
            IEnumerator Tween()
            {
                var minMax = _cinemachineBasedController.GetMinMaxValues(property);
                var tweenTime = 0f;
                do
                {
                    tweenTime += Time.deltaTime;
                    var tweenValue = Mathf.Lerp(tweenStartValue, endValue, tweenTime / time);

                    if (interpolateLoop)
                    {
                       var range = Mathf.Abs(minMax.y - minMax.x);
                        
                        if (tweenValue > minMax.y)
                        {
                            tweenValue -= range;
                            tweenStartValue -= range;
                            endValue = value;
                            interpolateLoop = false;
                        }
                        else if(tweenValue < minMax.x)
                        {
                            tweenValue += range;
                            tweenStartValue += range;
                            endValue = value;
                            interpolateLoop = false;
                        }
                    }
                    
                    _cinemachineBasedController.Set(property, tweenValue);
                    
                    yield return null;
                } 
                while (tweenTime < time);
                
                _cinemachineBasedController.Set(property, value);
                
                onComplete?.Invoke();
            }
        }

        public float GetValueOf(CameraAnimationProperty property)
        {
            return _cinemachineBasedController.GetValue(property);
        }

        public Vector2 GetMinMaxValuesOf(CameraAnimationProperty property)
        {
            return _cinemachineBasedController.GetMinMaxValues(property);
        }

        public CinemachineTargetGroup GetCinemachineLookAtTargetGroup()
        {
            return _cinemachineBasedController.GetCinemachineLookAtTargetGroup();
        }

        public GameObject GetLookAtTargetGroupGameObject()
        {
            return _cinemachineBasedController.GetLookAtTargetGroupGameObject();
        }

        public CinemachineTargetGroup GetCinemachineFollowTargetGroup()
        {
            return _cinemachineBasedController.GetCinemachineFollowTargetGroup();
        }

        public GameObject GetFollowTargetGroupGameObject()
        {
            return _cinemachineBasedController.GetFollowTargetGroupGameObject();
        }

        public Task ClearCachedFilesAsync()
        {
            return _animationSaver.CleanCacheAsync();
        }

        public void ForceUpdate()
        {
            var activeController = _cameraControllers.FirstOrDefault(x => x.IsActive);
            activeController?.ForceUpdate();
        }

        public void FocusOnTargetForce()
        {
            var isLookAtEnabled = _cinemachineBasedController.IsLookingAt;
            var isFollowEnabled = _cinemachineBasedController.IsFollowing;
            
            _cinemachineBasedController.SetLookAt(true);
            _cinemachineBasedController.SetFollow(true);
            
            //workaround for FREV-6562: need to force cinemachine update twice to get proper position
            ForceCinemachineUpdateTwice();

            _cinemachineBasedController.SetLookAt(isLookAtEnabled);
            _cinemachineBasedController.SetFollow(isFollowEnabled);
        }

        public void EnableCollider(bool isEnabled) => _cinemachineBasedController.EnableCollider(isEnabled);

        public void AllowCameraForMultiTimeSimulationInSingleFrame(bool isOn)
        {
            _multiTimePerFrameUpdatingEnabled = isOn;
            CinemachineCore.UniformDeltaTimeOverride = isOn
                ? 0.033f //30fps delta time
                : -1;
        }

        public void PutCinemachineToState(CameraAnimationFrame state)
        {
            foreach (var cameraAnimPropData in state)
            {
                var propType = cameraAnimPropData.Key;
                var propValue = cameraAnimPropData.Value;
                _cinemachineBasedController.Set(propType, propValue);
            }
        }

        public CameraAnimationFrame GetCurrentTransformBasedCameraState()
        {
            var frameValues = GetTransformBasedStateValues();
            return new CameraAnimationFrame(frameValues);
        }

        public CameraAnimationFrame GetCameraState()
        {
            var frameValues = GetTransformBasedStateValues();
            foreach (CameraAnimationProperty prop in Enum.GetValues(typeof(CameraAnimationProperty)))
            {
                if(prop.IsTransformProperty()) continue;
                
                frameValues[prop] = _cinemachineBasedController.GetValue(prop);
            }
            return new CameraAnimationFrame(frameValues);
        }

        public float GetXAxisValueToFocusOnPoint(Transform point)
        {
            return point.eulerAngles.y + 180 - _cinemachineBasedController.GetHeadingBias();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnNoiseProfileChanged(int id)
        {
            NoiseProfileChanged?.Invoke(id);
        }

        private void OnCameraUpdated()
        {
            CameraUpdated?.Invoke();
        }

        private void SwitchToCinemachineControl()
        {
            if(CinemachineEnabled) return;
            
            _transformBasedController.Enable(false);
            _cinemachineBasedController.Enable(true);

            var notCinemachineBasedAnimRunning = _animator.IsPlaying &&
                                              _animator.CurrentPlayingAnimationType != AnimationType.CinemachineBased;
            if(notCinemachineBasedAnimRunning)
                _animator.Stop();
        }

        private void StopControlCamera()
        {
            foreach (var cameraController in _cameraControllers)
            {
                cameraController.Enable(false);
            }
        }

        private void EnableIfNotEnabled()
        {
            if(_isEnabled) return;
            Enable(true);
        }

        private void AllowImmediateStateRecalculationIfEnabled()
        {
            if (!_multiTimePerFrameUpdatingEnabled) return;
            AllowImmediateCinemachineStateRecalculationOnceMore();
        }
        
        private void AllowImmediateCinemachineStateRecalculationOnceMore()
        {
            //we need to apply different delta times to cinemachine to force it update state
            //because cinemachine caсhes deltaTime value and prevents twice updating in the same frame
            //value doesn't matter, but it must be different from previous and non negative
            CinemachineCore.UniformDeltaTimeOverride += 0.01f;//by adding 0.01f we guarantee value is not the same as before
        }

        private void SetTargetsToCinemachine(GameObject lookAt, GameObject follow, bool recenter = true)
        {
            _cinemachineBasedController.SetTarget(lookAt?.transform);
            _cinemachineBasedController.SetFollowTarget(follow, recenter);
        }
        
        private void PrepareTargetCamera(Camera targetCamera)
        {
            if (targetCamera == null) return;
            targetCamera.nearClipPlane = CameraSystemConstants.FreeLookCamera.NEAR_CLIP_PLANE;
            targetCamera.farClipPlane = CameraSystemConstants.FreeLookCamera.FAR_CLIP_PLANE;
        }
        
        private void ForgetAnchor()
        {
            SetCameraAnchor(null);
            _cameraAnchor = null;
        }

        private void SubscribeToTargetCameraDestroyEvent()
        {
            if(_targetCamera == null) return;
            
            _targetCamera.gameObject.AddListenerToDestroyEvent(OnCameraDestroyed);
        }
        
        private void UnsubscribeFromTargetCameraDestroyEvent()
        {
            if(_targetCamera == null) return;
            
            _targetCamera.gameObject.RemoveListenerFromDestroyEvent(OnCameraDestroyed);
        }

        private void OnCameraDestroyed()
        {
            StopControlCamera();
            ForgetCamera();
        }
        
        private void SetupControllers(Camera camera, CinemachineBrain brain)
        {
            _cinemachineBasedController.SetCamera(camera, brain);
            _transformBasedController.SetCamera(camera);
        }
        
        private void ForceCinemachineUpdateTwice()
        {
            AllowCameraForMultiTimeSimulationInSingleFrame(true);
            _cinemachineBasedController.ForceUpdate();
            AllowImmediateCinemachineStateRecalculationOnceMore();
            _cinemachineBasedController.ForceUpdate();
            AllowCameraForMultiTimeSimulationInSingleFrame(false);
        }
        
        private void OnApplicationQuit()
        {
            UnsubscribeFromTargetCameraDestroyEvent();
        }
        
        private Dictionary<CameraAnimationProperty, float> GetTransformBasedStateValues()
        {
            var position = _cameraAnchor.InverseTransformPoint(_targetCamera.transform.position);
            var worldSpaceRot = _targetCamera.transform.rotation;
            var localRot = (Quaternion.Inverse(_cameraAnchor.rotation) * worldSpaceRot).eulerAngles;

            var frameValues = new Dictionary<CameraAnimationProperty, float>
            {
                { CameraAnimationProperty.PositionX, position.x },
                { CameraAnimationProperty.PositionY, position.y },
                { CameraAnimationProperty.PositionZ, position.z },
                { CameraAnimationProperty.RotationX, localRot.x },
                { CameraAnimationProperty.RotationY, localRot.y },
                { CameraAnimationProperty.RotationZ, localRot.z }
            };
            return frameValues;
        }
    }
}