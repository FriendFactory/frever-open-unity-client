using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Extensions;
using Modules.CameraSystem.CameraAnimations;
using Modules.CameraSystem.CameraSystemCore;
using Modules.CameraSystem.Extensions.CinemachineExtensions;
using Modules.CameraSystem.PlayerCamera.Handlers;
using UnityEngine;
using Zenject;

namespace Modules.CameraSystem.PlayerCamera
{
    internal sealed class CinemachineBasedController : MonoBehaviour, ICameraController, ILateTickable
    {
        private const float COLLIDER_DAMPING = 0.1f;
        private const float COLLIDER_OCCLUDED_DAMPING = 0.1f;
        private const float COLLIDER_SMOOTHING_TIME = 0.2f;
        private const float BOTTOM_ORBIT_HEIGHT = 0.1f;
        private const float TOP_RIG_RADIUS = 0.5f;
        
        private static readonly Vector2 DeadZoneSize = new Vector2(CameraSystemConstants.CameraComposer.DEAD_ZONE_WIDTH, CameraSystemConstants.CameraComposer.DEAD_ZONE_HEIGHT);

        private Camera _camera;
        private CinemachineFreeLook _cinemachineFreeLook;
        private CameraNoiseProfileHandler _cameraNoiseProfileHandler;
        private CameraDofManager _cameraDofManager;
        private CameraDofUpdater _cameraDofUpdater;
        private CinemachineBrain _cinemachineBrain;
        private CinemachineRelativeRotationKeeper _relativeRotationKeeper;
        private CinemachineTargetGroup _lookAtTargetGroup;
        private CinemachineTargetGroup _followTargetGroup;
        private GameObject _followTarget;
        private GameObject _tempTarget;
        private CinemachineVirtualCamera[] _rigs;
        private CinemachineCollider _cinemachineCollider;
        private Transform _anchor;
        private CameraFocusTargetAdjuster _focusTargetAdjuster;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public bool IsLookingAt { get; private set; }
        public bool IsFollowing { get; private set; }
        public bool HasTarget { get; private set; }
        public float MaxRigHeight { get; set; }
        public Vector3 CameraPosition => _camera.transform.position;
        public NoiseSettings[] NoiseProfiles => _cameraNoiseProfileHandler.NoiseProfiles;
        public int CurrentNoiseProfileId => _cameraNoiseProfileHandler.CurrentNoiseProfileId;
        public AnimationType TargetAnimationType => AnimationType.CinemachineBased;
        public bool IsActive => enabled;
        public Transform Target { get; private set; }
        private CameraFocusTargetAdjuster FocusTargetAdjuster 
        {
            get
            {
                if (_focusTargetAdjuster == null)
                {
                    _focusTargetAdjuster = new GameObject("FocusTargetAdjuster").AddComponent<CameraFocusTargetAdjuster>();
                    CinemachineCore.CameraUpdatedEvent.AddListener(x=> _focusTargetAdjuster.UpdatePosition());
                    DontDestroyOnLoad(_focusTargetAdjuster.gameObject);
                }
                return _focusTargetAdjuster;
            }
        }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action AppliedStateToGameObject;
        public event Action<bool> ActivatedStatusChanged;
        public event Action<int> NoiseProfileChanged;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        [Inject]
        private void Construct(CinemachineFreeLook cinemachineFreeLook, CameraNoiseProfileHandler cameraNoiseProfileHandler, 
            CameraDofManager cameraDofManager, CameraDofUpdater cameraDofUpdater, CinemachineRelativeRotationKeeper relativeRotationKeeper)
        {
            _cinemachineFreeLook = cinemachineFreeLook;
            _cameraNoiseProfileHandler = cameraNoiseProfileHandler;
            _cameraDofManager = cameraDofManager;
            _cameraDofUpdater = cameraDofUpdater;
            _lookAtTargetGroup = new GameObject("LookAtTargetGroup").AddComponent<CinemachineTargetGroup>();
            _followTargetGroup = new GameObject("FollowTargetGroup").AddComponent<CinemachineTargetGroup>();
            _lookAtTargetGroup.m_UpdateMethod = CinemachineTargetGroup.UpdateMethod.LateUpdate;
            _cinemachineFreeLook.gameObject.transform.SetParent(gameObject.transform);
            _rigs = new[] {GetTopRig(), GetMiddleRig(), GetBottomRig()};
            _cinemachineFreeLook.m_Orbits[2].m_Height = BOTTOM_ORBIT_HEIGHT;
            _cinemachineFreeLook.m_Orbits[0].m_Radius = TOP_RIG_RADIUS;
            SetupCinemachineCollider();

            Enable(false);
            _cameraNoiseProfileHandler.NoiseProfileChanged += OnNoiseProfileChanged;
            _cameraNoiseProfileHandler.Setup(_rigs);
            _relativeRotationKeeper = relativeRotationKeeper;
            _relativeRotationKeeper.SetCinemachineTransform(cinemachineFreeLook.transform);
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetCamera(Camera unityCamera, CinemachineBrain brain)
        {
            _camera = unityCamera;
            _cinemachineBrain = brain;
            if (unityCamera != null)
            {
                FocusTargetAdjuster.CameraTransform = unityCamera.transform;
            }
        }

        public void SetAnchor(Transform anchor)
        {
            SyncFollowAndLookAtGroupsRotationWithAnchorRotation(anchor);
            TryAdaptRotationToNewAnchor(_anchor, anchor);
            _anchor = anchor;
        }
        
        private CinemachineVirtualCamera GetTopRig()
        {
            return _cinemachineFreeLook.GetRig(0);
        }

        private CinemachineVirtualCamera GetMiddleRig()
        {
            return _cinemachineFreeLook.GetRig(1);
        }

        private CinemachineVirtualCamera GetBottomRig()
        {
            return _cinemachineFreeLook.GetRig(2);
        }

        public CinemachineComposer GetCinemachineComposer()
        {
            return GetBottomRig().GetCinemachineComponent<CinemachineComposer>();
        }

        public void SetTarget(Transform target)
        {
            UnSubscribeFromCurrentTarget();
            
            SetTargetInternal(target);
            
            SubscribeToCurrentTarget();
        }

        public void SetFocusAdjustmentCurve(AnimationCurve animationCurve)
        {
            FocusTargetAdjuster.AdjustPositionCurve = animationCurve;
        }
        
        private void OnTargetDestroyed()
        {
            SetTargetInternal(null);
        }

        public void SetLookAt(bool value)
        {
            IsLookingAt = value;

            if (Target == null)
            {
                return;
            }

            if (!value)
            {
                _cinemachineFreeLook.LookAt = null;
                return;
            }

            FocusTargetAdjuster.Target = Target;
            _cinemachineFreeLook.LookAt = FocusTargetAdjuster.AdjustedTarget;
        }

        public void SetFollowTarget(GameObject target, bool recenter)
        {
            if (target == null)
            {
                _followTarget = null;
                _cinemachineFreeLook.Follow = null;
                return;
            }
            
            if (_cinemachineFreeLook.Follow == target.transform) return;

            _followTarget = target;
            if (!IsFollowing) return;

            _cinemachineFreeLook.Follow = target.transform;
            if (recenter)
            {
                RecenterFocus();
            }
        }

        public void SetFollow(bool value)
        {
            IsFollowing = value;
            if (_followTarget == null) return;

            if (!IsFollowing)
            {
                _cinemachineFreeLook.Follow = GetTemporaryTarget();
                UpdateTemporaryTargetPosition();
                return;
            }

            _cinemachineFreeLook.m_Follow = _followTarget.transform;
            ForgetTemporaryTarget();
        }

        public void UpdateFollowingState()
        {
            SetFollow(IsFollowing);
            UpdateTemporaryTargetPosition();
        }

        /// <summary>
        /// Default camera offset to be in front on target is subtracted with new offset.
        /// </summary>
        public void SetHeadingBias(float value)
        {
            _cinemachineFreeLook.m_Heading.m_Bias = value - CameraSystemConstants.FreeLookCamera.HEADING_BIAS;
        }

        public float GetHeadingBias()
        {
            return _cinemachineFreeLook.m_Heading.m_Bias;
        }

        public float GetRigRadius()
        {
            return _cinemachineFreeLook.m_Orbits[1].m_Radius;
        }

        public float DistanceToTarget()
        {
            return (_cinemachineFreeLook.State.FinalPosition - Target.position).magnitude;
        }

        public void Enable(bool active)
        {
            if(active == enabled) return;
            _cinemachineFreeLook.enabled = active;
            enabled = active;
            _cameraDofUpdater.Enable(active);

            if (active)
            {
                InheritCameraGameObjectPosition();
                InheritFieldOfView();
                AdaptToAnchorRotation();
                UpdateFollowingState();
                CinemachineCore.CameraUpdatedEvent.AddListener(OnStateApplied);
            }
            else
            {
                CinemachineCore.CameraUpdatedEvent.RemoveListener(OnStateApplied);
            }
            
            ActivatedStatusChanged?.Invoke(active);
        }

        public CinemachineTargetGroup GetCinemachineLookAtTargetGroup()
        {
            return _lookAtTargetGroup;
        }
        
        public GameObject GetLookAtTargetGroupGameObject()
        {
            return _lookAtTargetGroup.gameObject;
        }
        
        public CinemachineTargetGroup GetCinemachineFollowTargetGroup()
        {
            return _followTargetGroup;
        }
        
        public GameObject GetFollowTargetGroupGameObject()
        {
            return _followTargetGroup.gameObject;
        }

        public void RecenterFocus()
        {
            StartCoroutine(Recenter());
        }
        
        public float GetValue(CameraAnimationProperty property)
        {
            switch (property)
            {
                case CameraAnimationProperty.AxisX:
                    return GetXAxis();
                case CameraAnimationProperty.AxisY:
                    return GetYAxis();
                case CameraAnimationProperty.OrbitRadius:
                    return GetRigRadius();
                case CameraAnimationProperty.HeightRadius:
                    return GetRigHeight();
                case CameraAnimationProperty.FieldOfView:
                    return GetFieldOfView();
                case CameraAnimationProperty.Dutch:
                    return GetDutch();
                case CameraAnimationProperty.DepthOfField:
                    return GetDepthOfField();
                case CameraAnimationProperty.PositionX:
                    return GetRelativePosition().x;
                case CameraAnimationProperty.PositionY:
                    return GetRelativePosition().y;
                case CameraAnimationProperty.PositionZ:
                    return GetRelativePosition().z;
                case CameraAnimationProperty.RotationX:
                    return GetRelativeRotation().x;
                case CameraAnimationProperty.RotationY:
                    return GetRelativeRotation().y;
                case CameraAnimationProperty.RotationZ:
                    return GetRelativeRotation().z;
                case CameraAnimationProperty.FocusDistance:
                    return DistanceToTarget();
                default:
                    throw new ArgumentOutOfRangeException(nameof(property), property, null);
            }
        }

        public void Set(CameraAnimationProperty property, float value)
        {
            switch (property)
            {
                case CameraAnimationProperty.AxisX:
                    SetXAxis(value);
                    break;
                case CameraAnimationProperty.AxisY:
                    SetYAxis(value);
                    break;
                case CameraAnimationProperty.OrbitRadius:
                    SetRigRadiusAndHeight(value);
                    break;
                case CameraAnimationProperty.HeightRadius:
                    SetUpperRigHeight(value);
                    break;
                case CameraAnimationProperty.FieldOfView:
                    SetFieldOfView(value);
                    break;
                case CameraAnimationProperty.Dutch:
                    SetDutch(value);
                    break;
                case CameraAnimationProperty.DepthOfField:
                    _cameraDofManager.SetDepthOfField(value);
                    break;
                case CameraAnimationProperty.FocusDistance:
                    break;
            }
        }
        
        public void ForceUpdate()
        {
            _lookAtTargetGroup.DoUpdate();
            _followTargetGroup.DoUpdate();
            _cinemachineBrain.ForceUpdate();
        }
        
        public void SetLookAtGroupMembers(IEnumerable<Transform> members)
        {
            var lookAtTargetGroup = GetCinemachineLookAtTargetGroup();
            lookAtTargetGroup.m_Targets = null;

            foreach (var target in members)
            {
                lookAtTargetGroup.AddMember(target, 1f, 0f);
            }
        }

        public void SetFollowGroupMembers(IEnumerable<Transform> members)
        {
            var followTargetGroup = GetCinemachineFollowTargetGroup();
            followTargetGroup.m_Targets = null;
            
            foreach (var target in members)
            {
                followTargetGroup.AddMember(target, 1f, 0f);
            }
        }
        
        public void SetNoiseProfile(int cameraNoiseIndex)
        {
            _cameraNoiseProfileHandler.SetNoiseProfile(cameraNoiseIndex);
        }

        public void SetFocusDistance(float value)
        {
            _cameraDofManager.SetFocusDistance(value);
        }
        
        public void UpdateStateBasedOnCurrentPosition()
        {
            InheritCameraGameObjectPosition();
        }

        public void ForgetTemporaryTarget()
        {
            Destroy(_tempTarget);
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        public void LateTick()
        {
            if(!enabled) return;
            
            UpdateCinemachineCameraState();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SetupCinemachineCollider()
        {
            _cinemachineCollider = _cinemachineFreeLook.gameObject.AddOrGetComponent<CinemachineCollider>();
            SetColliderDamping(COLLIDER_DAMPING, COLLIDER_OCCLUDED_DAMPING, COLLIDER_SMOOTHING_TIME);
            _cinemachineCollider.m_Strategy = CinemachineCollider.ResolutionStrategy.PullCameraForward;
            _cinemachineCollider.m_CollideAgainst = LayerMask.GetMask("Environment");
        }

        private void SetColliderDamping(float damping, float occludedDamping, float smoothingTime)
        {
            _cinemachineCollider.m_Damping = damping;
            _cinemachineCollider.m_DampingWhenOccluded = occludedDamping;
            _cinemachineCollider.m_SmoothingTime = smoothingTime;
        }

        private IEnumerator Recenter()
        {
            //tricky way of re-centering camera on Cinemachine via dead zone size
            SetDeadZone(Vector2.zero);
            yield return new WaitForEndOfFrame();
            SetDeadZone(DeadZoneSize);
        }

        private void SetDeadZone(Vector2 size)
        {
            foreach (var rig in _rigs)
            {
                var composer = rig.GetCinemachineComponent<CinemachineComposer>();
                composer.m_DeadZoneWidth = size.x;
                composer.m_DeadZoneHeight = size.y;
            }
        }

        private void InheritFieldOfView()
        {
            SetFieldOfView(_camera.fieldOfView);
        }
        
        private void UpdateCinemachineCameraState()
        {
            _cinemachineFreeLook.InternalUpdateCameraState(Vector3.up, 0);
        }

        private Vector3 GetRelativePosition()
        {
            var camWorldSpace = _camera.transform.position;
            return _anchor.InverseTransformPoint(camWorldSpace);
        }

        private Vector3 GetRelativeRotation()
        {
            var worldSpaceRot = _camera.transform.rotation;
            return (Quaternion.Inverse(_anchor.rotation) * worldSpaceRot).eulerAngles;
        }

        private void OnStateApplied(CinemachineBrain brain)
        {
            AppliedStateToGameObject?.Invoke();
        }
        
        private void UpdateMiddleRigHeight()
        {
            _cinemachineFreeLook.m_Orbits[1].m_Height = Mathf.Abs(_cinemachineFreeLook.m_Orbits[0].m_Height + _cinemachineFreeLook.m_Orbits[2].m_Height) / 2;
        }

        private Transform GetTemporaryTarget()
        {
            if (_tempTarget != null) return _tempTarget.transform;
            
            _tempTarget = new GameObject("TempLookAtTarget");
            _tempTarget.transform.position = _followTarget.transform.position;
            _tempTarget.transform.eulerAngles = _followTarget.transform.eulerAngles;
            DontDestroyOnLoad(_tempTarget);

            return _tempTarget.transform;
        }
        
        private float GetDepthOfField()
        {
            return _cameraDofManager.GetDepthOfField();
        }

        private void OnNoiseProfileChanged(int id)
        {
            NoiseProfileChanged?.Invoke(id);
        }

        private void SetFieldOfView(float fov)
        {
            _cinemachineFreeLook.m_Lens.FieldOfView = fov;
        }

        private float GetFieldOfView()
        {
            return _cinemachineFreeLook.m_Lens.FieldOfView;
        }

        private void SetDutch(float dutch)
        {
            _cinemachineFreeLook.m_Lens.Dutch = dutch;
        }

        private float GetDutch()
        {
            return _cinemachineFreeLook.m_Lens.Dutch;
        }
        
        private void SetYAxis(float value)
        {
            _cinemachineFreeLook.m_YAxis.Value = value;
        }

        private float GetYAxis()
        {
            return _cinemachineFreeLook.m_YAxis.Value;
        }

        private float GetXAxis()
        {
            return _cinemachineFreeLook.m_XAxis.Value;
        }

        private void SetXAxis(float value)
        {
            _cinemachineFreeLook.m_XAxis.Value = value;
        }

        private void SetRigRadiusAndHeight(float value)
        {
            _cinemachineFreeLook.m_Orbits[1].m_Radius = value;
            _cinemachineFreeLook.m_Orbits[2].m_Radius = value;

            var rigHeight = Mathf.Clamp(value, 2f, MaxRigHeight);
            SetUpperRigHeight(rigHeight);
        }

        private void SetUpperRigHeight(float value)
        {
            _cinemachineFreeLook.m_Orbits[0].m_Height = value;
            UpdateMiddleRigHeight();
        }
        
        private float GetRigHeight()
        {
            return _cinemachineFreeLook.m_Orbits[0].m_Height;
        }

        private void InheritCameraGameObjectPosition()
        {
            //Workaround for CinemachineFreeLook inheritance. It works not correct for inheritance if dutch > 90 degrees.
            //To prevent that we just set dutch and z euler angle to 0,then do inheritance and then we restore the dutch(FREV-6077)
            var camTransform = _camera.transform;
            var rotZBeforeInheriting = camTransform.eulerAngles.z;
            camTransform.SetEulerAngleZ(0);
            
            _cinemachineFreeLook.InheritPositionForce(_camera, Vector3.up);
            
            var dutch = CinemachineFreeLookUtils.EulerAngleZToDutch(rotZBeforeInheriting);
            SetDutch(dutch);
        }

        private void SyncFollowAndLookAtGroupsRotationWithAnchorRotation(Transform anchor)
        {
            if(anchor == null) return;

            var anchorRotation = anchor.rotation;
            _followTargetGroup.Transform.rotation = anchorRotation;
            _lookAtTargetGroup.Transform.rotation = anchorRotation;
        }

        private void TryAdaptRotationToNewAnchor(Transform previousAnchor, Transform nextAnchor)
        {
            _relativeRotationKeeper.TrackSwitchedAnchors(previousAnchor, nextAnchor);
            //if not active - it should be applied on cinemachine activation just after position inheriting 
            if (IsActive)
            {
                AdaptToAnchorRotation();
            }
        }

        private void AdaptToAnchorRotation()
        {
            _relativeRotationKeeper.AdaptRotationToNewAnchor();
        }

        private void SetTargetInternal(Transform target)
        {
            HasTarget = target != null;
            Target = target;
            
            RefreshLookAtState();
        }

        private void RefreshLookAtState()
        {
            if (!IsLookingAt) return;
            FocusTargetAdjuster.Target = Target;
            _cinemachineFreeLook.LookAt = FocusTargetAdjuster.AdjustedTarget;
        }
        
        private void SubscribeToCurrentTarget()
        {
            if (!HasTarget) return;
            Target.AddListenerToDestroyEvent(OnTargetDestroyed);
        }
        
        private void UnSubscribeFromCurrentTarget()
        {
            if (!HasTarget) return;
            Target.RemoveListenerFromDestroyEvent(OnTargetDestroyed);
        }
        
        /// <summary>
        /// Update temporary camera follow target position with original follow target position.
        /// Used before applying heading bias to make sure it is applied correctly.
        /// </summary>
        private void UpdateTemporaryTargetPosition()
        {
            if (_tempTarget == null || _followTarget == null) return;
            
            //Make sure follow target group has correct position before syncing position. Required if group target is used. 
            _followTargetGroup.DoUpdate();
            
            _tempTarget.transform.position = _followTarget.transform.position;
            _tempTarget.transform.eulerAngles = _followTarget.transform.eulerAngles;
        }

        public Vector2 GetMinMaxValues(CameraAnimationProperty property)
        {
            return property switch
            {
                CameraAnimationProperty.AxisX => new(_cinemachineFreeLook.m_XAxis.m_MinValue, _cinemachineFreeLook.m_XAxis.m_MaxValue),
                CameraAnimationProperty.AxisY => new(_cinemachineFreeLook.m_YAxis.m_MinValue, _cinemachineFreeLook.m_YAxis.m_MaxValue),
                _ => Vector2.zero
            };
        }

        public void EnableCollider(bool isEnabled)
        {
            var component = _cinemachineFreeLook.GetComponent<CinemachineCollider>();
            if (component) component.enabled = isEnabled;
        }
    }
}