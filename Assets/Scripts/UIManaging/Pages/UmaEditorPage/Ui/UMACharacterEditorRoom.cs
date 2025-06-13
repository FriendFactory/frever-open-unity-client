using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Bridge.ExternalPackages.AsynAwaitUtility;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Common;
using Extensions;
using Modules.FreverUMA;
using Modules.PhotoBooth.Character;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;
using Zenject;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    public sealed class UMACharacterEditorRoom : MonoBehaviour
    {
        private readonly float[] _goodAnimationPoints = { 0f, 0.1f, 0.25f, 0.3f, 0.4f, 0.55f, 0.6f, 0.7f, 0.85f, 0.9f };
        
        [SerializeField] private CharacterPhotoBooth _characterPhotoBooth;
        
        public Action<float> ZoomChanged;

        public Transform UMAEditorAvatarPlaceholder;
        public RuntimeAnimatorController roomCharacterController;
        public Camera roomCamera;
        public GameObject roomLights;

        [SerializeField] private EditorZoomingPresetsContainer[] _presetsContainers;
        
        private BodyDisplayMode _bodyDisplayMode = BodyDisplayMode.FullBody;
        private RectTransform _rawImageRect;
        private EditorCameraController _cameraController;
        [Inject] private AvatarHelper _avatarHelper;
        private float _placeHolderOriginLocalPosY;
        private Race _race;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public DynamicCharacterAvatar Avatar { get; private set; }

        public BodyDisplayMode BodyDisplayMode
        {
            get => _bodyDisplayMode;
            set
            {
                if (BodyDisplayMode == value) return;
                _bodyDisplayMode = value;
                RefreshCamera();
            }
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _placeHolderOriginLocalPosY = UMAEditorAvatarPlaceholder.transform.localPosition.y;
        }

        private void Start()
        {
            _cameraController = roomCamera.GetComponent<EditorCameraController>();
            _cameraController.ZoomChanged += (value) => ZoomChanged?.Invoke(value);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetRawImageRect(RectTransform rectTransform)
        {
            _rawImageRect = rectTransform;
        }
        
        public void DespawnAvatar()
        {
            if(Avatar == null) return;
            Avatar.CharacterCreated.RemoveListener(OnCharacterUpdated);
            Avatar.CharacterUpdated.RemoveListener(AdjustCharacterPosition);
            Destroy(Avatar.gameObject);
            Avatar = null;
        }

        public void Show(DynamicCharacterAvatar avatar, Race race, bool disablePinchZoom = false)
        {
            _race = race;
            Avatar = avatar;
            Avatar.transform.SetParent(UMAEditorAvatarPlaceholder);
            Avatar.transform.localPosition = Vector3.zero;
            Avatar.raceAnimationControllers.defaultAnimationController = roomCharacterController;
            Avatar.CharacterUpdated.AddListener(SetBigBounds);

            UMAEditorAvatarPlaceholder.gameObject.SetActive(true);
            roomCamera.gameObject.SetActive(true);
            roomLights.SetActive(true);
            _cameraController.Setup(_rawImageRect, disablePinchZoom);
            
            Avatar.CharacterCreated.AddListener(OnCharacterUpdated);
            Avatar.CharacterUpdated.AddListener(AdjustCharacterPosition);
        }

        public void Hide()
        {
            UMAEditorAvatarPlaceholder.gameObject.SetActive(false);
            roomLights.SetActive(false);
            roomCamera.gameObject.SetActive(false);
            roomCamera.targetTexture.Release();
        }

        private EditorZoomingPreset GetCameraPreset(BodyDisplayMode displayMode)
        {
            if (_race is null)
            {
                return _presetsContainers[0].Presets.FirstOrDefault(preset => preset.DisplayMode == displayMode);
            }
            return _presetsContainers.FirstOrDefault(c => c.RaceId == _race.Id).Presets
                .FirstOrDefault(preset => preset.DisplayMode == displayMode);
        }

        [SuppressMessage("ReSharper", "Unity.InefficientPropertyAccess")]
        public async Task<Texture2D> GetSnapshot(BodyDisplayMode bodyDisplayMode = BodyDisplayMode.FullBody) 
        {
            await SetTheFirstAnimationFrame();

            return await _characterPhotoBooth.GetPhotoAsync(_race.Id, bodyDisplayMode, UMAEditorAvatarPlaceholder.position, Avatar.transform.rotation, true);
        }

        public void ResetZoomingSilent()
        {
            _cameraController.ResetZoom();
            var preset = GetCameraPreset(BodyDisplayMode);
            _cameraController.SetPositionSilent(preset);
        }

        public void SetZoom(float newZoom)
        {
            _cameraController.SetZoom(newZoom);
        }

        private void SetCameraLocation(BodyDisplayMode displayMode)
        {
            var preset = GetCameraPreset(displayMode);
            _cameraController.SetDestination(preset);
        }

        private void SetBigBounds(UMAData data)
        {
            var skinnedMesh = Avatar.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinnedMesh == null) return;
            skinnedMesh.localBounds = new Bounds(new Vector3(), Vector3.one * 500);
        }

        private void OnCharacterUpdated(UMAData data)
        {
            UpdateCameraTargets();
            RefreshCamera();
        }

        private void UpdateCameraTargets()
        {
            var container = _presetsContainers.FirstOrDefault(c => c.RaceId == _race.Id).Presets;
                
            foreach (var preset in container)
            {
                if (string.IsNullOrEmpty(preset.TargetBoneName)) continue;

                var boneTransform = Avatar.transform.FindChildInHierarchy(preset.TargetBoneName);
                if (boneTransform != null)
                {
                    preset.ZoomingTarget = boneTransform;
                }
            }
        }
        
        private void AdjustCharacterPosition(UMAData umaData)
        {
            var heelsHeight = _avatarHelper.GetHeelsHeight(Avatar);
            var localPositionToPlaceExactOnFloor = _placeHolderOriginLocalPosY + heelsHeight;
            UMAEditorAvatarPlaceholder.SetLocalPositionY(localPositionToPlaceExactOnFloor);
        }
        
        private void RefreshCamera()
        {
            SetCameraLocation(_bodyDisplayMode);
        }

        private async Task SetTheFirstAnimationFrame()
        {
            var animator = Avatar.GetComponent<Animator>(); 
            
            float currentTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f;
            float closestTime = FindClosestAnimationPoint(currentTime);
            
            animator.Play(Constants.CharacterEditor.START_BODY_ANIM_NAME, 0, closestTime);
            animator.Update(0.0001f);
            animator.speed = 0f;
            // we have to wait for an and of frame to apply the animation for the camera render
            await new WaitForEndOfFrame();
        }

        private float FindClosestAnimationPoint(float currentTime)
        {
            float closestPoint = _goodAnimationPoints[0];
            float minDifference = Mathf.Abs(currentTime - _goodAnimationPoints[0]);
    
            foreach (float point in _goodAnimationPoints)
            {
                float difference = Mathf.Abs(currentTime - point);
                if (difference < minDifference)
                {
                    minDifference = difference;
                    closestPoint = point;
                }
            }
    
            return closestPoint;
        }
    }

    [Serializable]
    public class EditorZoomingPreset
    {
        public BodyDisplayMode DisplayMode;
        public Transform ZoomingTarget;
        public float ZoomValue;
        public float ZoomValueForNarrowScreen;
        public string TargetBoneName;
        [FormerlySerializedAs("Offset")] public Vector3 TargetOffset;
        public Vector3 CameraOffset;
        public Vector3 CameraOffsetForNarrowScreen;

        public Vector3 TargetPosition => ZoomingTarget.position + TargetOffset;
        
        public Vector3 GetInterpolatedCameraOffset(float t)
        {
            return CameraOffset + (CameraOffsetForNarrowScreen - CameraOffset) * t;
        } 

        public float GetInterpolatedZoomValue(float t)
        {
            return ZoomValue + (ZoomValueForNarrowScreen - ZoomValue) * t;
        } 

    }

    [Serializable]
    public class EditorZoomingPresetsContainer
    {
        public long RaceId;
        public EditorZoomingPreset[] Presets;
    }
}