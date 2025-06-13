using Common.Permissions;
using Extensions;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.Permissions;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.FaceTrackingToggle
{
    internal sealed class FaceTrackingToggleButton : MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private Image _toggleImage;
        [SerializeField] private Sprite _onSprite;
        [SerializeField] private Sprite _offSprite;

        [Inject] private ILevelManager _levelManager;
        [Inject] private LevelEditorPageModel _levelEditorPageModel;
        [Inject] private CameraPermissionHandler _cameraPermissionHandler;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        private bool IsFaceRecordingEnabled =>
            _levelManager.IsFaceRecordingEnabled &&
            _cameraPermissionHandler.IsPermissionGranted;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            RefreshVisuals();

            _levelManager.FaceRecordingStateChanged += RefreshVisuals;
            _cameraPermissionHandler.OpenAppSettingsRequestCanceled += RefreshVisuals;

            _toggle.onValueChanged.AddListener(OnToggleValueChanged);
            _toggleImage.SetNativeSize();
        }

        private void OnDestroy()
        {
            _levelManager.FaceRecordingStateChanged -= RefreshVisuals;
            _cameraPermissionHandler.OpenAppSettingsRequestCanceled -= RefreshVisuals;
            _toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void RefreshFaceTrackStatus()
        {
            if (_levelManager.TargetEvent == null) return;
            
            var hasFaceAnimationAsset = _levelManager.TargetEvent.HasFaceAnimation();
            var shouldFaceTrackBeEnabled = _levelManager.CurrentLevel.Event.Count == 0 || hasFaceAnimationAsset;
            _toggle.isOn = shouldFaceTrackBeEnabled; 
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void RefreshVisuals()
        {
            _toggle.SetIsOnWithoutNotify(IsFaceRecordingEnabled);
            _toggleImage.sprite = IsFaceRecordingEnabled ? _onSprite : _offSprite;
            _toggleImage.SetNativeSize();
        }

        private void OnToggleValueChanged(bool value)
        {
            if (value && !_cameraPermissionHandler.IsPermissionGranted)
            {
                RequestPermission();
                return;
            }
            
            UpdateFaceRecordingState(value);
            
            void RequestPermission()
            {
                _cameraPermissionHandler.RequestPermission(OnPermissionRequested);

                void OnPermissionRequested(PermissionRequestResult result)
                {
                    if (result.IsError || result.IsSkipped)
                    {
                        UpdateFaceRecordingState(false);
                        return;
                    }
                    
                    UpdateFaceRecordingState(result.PermissionStatus.IsGranted());
                }
            }
        }

        private void UpdateFaceRecordingState(bool isEnabled)
        {
            // we need to perform additional permission check in order to prevent permission request from ARFace manager 
            _levelManager.SetFaceRecording(isEnabled && _cameraPermissionHandler.IsPermissionGranted);
        }
    }
}