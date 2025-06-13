using System;
using Cinemachine;
using Modules.InputHandling;
using Modules.LevelManaging.Assets;
using Modules.PhotoBooth.Profile;
using UnityEngine;
using Zenject;

namespace Modules.ProfilePhotoEditing
{
    internal sealed class VirtualCameraBasedController : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;
        [SerializeField] private Transform _target;

        private Transform LookAt { get; set; }
        private Transform Follow { get; set; }

        [Inject] private IInputManager _inputManager;

        private CinemachineTransposer _transposer;
        private ProfilePhotoBoothPreset _cameraSettings;

        //---------------------------------------------------------------------
        // Messages 
        //---------------------------------------------------------------------

        private void Awake()
        {
            _transposer = _virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
            
            _inputManager.ZoomBegin += StartUpdateFollowOffsetZ;
            _inputManager.ZoomExecuting += UpdateFollowOffsetZ;
            _inputManager.ZoomEnd += EndUpdateFollowOffsetZ;
        }
        
        private void OnDestroy()
        {
            _inputManager.ZoomBegin -= StartUpdateFollowOffsetZ;
            _inputManager.ZoomExecuting -= UpdateFollowOffsetZ;
            _inputManager.ZoomEnd -= EndUpdateFollowOffsetZ;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetCameraTargets(Transform lookAt, Transform follow)
        {
            LookAt = lookAt;
            Follow = follow;

            UpdateTargetTransform();
        }
        
        public void SetTargetParent(Transform parent)
        {
            _target.parent = parent;
        }

        public void UpdateTargetTransform()
        {
            var position = Follow.position;
            position.y = LookAt.position.y;
            position += _cameraSettings.targetOffset;
            var rotation = new Vector3(0f, Follow.rotation.eulerAngles.y, 0f);
            
            _target.SetPositionAndRotation(position, Quaternion.Euler(rotation));
        }

        public void UpdateCameraSettings(ProfilePhotoBoothPreset preset)
        {
            _cameraSettings = preset;

            _transposer.m_FollowOffset = _cameraSettings.followOffset;
            _virtualCamera.m_Lens.FieldOfView = _cameraSettings.verticalFOV;
        }

        //---------------------------------------------------------------------
        // Helpers 
        //---------------------------------------------------------------------

        private void StartUpdateFollowOffsetZ() { }

        private void UpdateFollowOffsetZ(float value)
        {
            var followMin = _cameraSettings.followOffset.z * 0.5f;
            var followMax = _cameraSettings.followOffset.z * 1.5f;
            var currentValue = _transposer.m_FollowOffset.z;
            _transposer.m_FollowOffset.z = Mathf.Clamp(currentValue * value, followMin, followMax);
        }

        private void EndUpdateFollowOffsetZ() { }

    }
}