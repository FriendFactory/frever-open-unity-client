using Common;
using Common.Permissions;
using JetBrains.Annotations;
using Modules.FaceAndVoice.Voice.Recording.Core;
using UIManaging.PopupSystem;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.Permissions
{
    [UsedImplicitly]
    internal sealed class MicrophonePermissionHelper: PermissionHelperBase 
    {
        private static readonly string MICROPHONE_CALIBRATED_ID = $"{Application.identifier}.CalibratedMicrophoneId";
        
        protected override PermissionTarget PermissionTarget => PermissionTarget.Microphone;
        
        private readonly LevelEditorPageModel _pageModel;
        private readonly  IVoiceRecorder _voiceRecorder;

        public MicrophonePermissionHelper(IPermissionsHelper permissionsHelper,
            LevelEditorPageModel levelEditorPageModel, IVoiceRecorder voiceRecorder, PopupManagerHelper popupManagerHelper) : base(permissionsHelper, popupManagerHelper)
        {
            _pageModel = levelEditorPageModel;
            _voiceRecorder = voiceRecorder;
        }

        public override void Initialize()
        {
            base.Initialize();
            
        #if UNITY_ANDROID && !UNITY_EDITOR
            if (!PermissionStatus.IsGranted()) return;

            if (!PlayerPrefs.HasKey(MICROPHONE_CALIBRATED_ID))
            {
                RunMicrophoneCalibration();
                return;
            }

            SetMicrophone();
        #endif
        }

        protected override void RequestPermissionInternal()
        {
            switch (PermissionStatus)
            {
                case PermissionStatus.NotDetermined:
                    RequestTargetPermission();
                    break;
                case PermissionStatus.Denied:
                    RequestOpenAppPermissionMenuWithPopup(LevelEditorPopupLocalization.MicrophonePermissionDeniedPopupTitle,
                                                          LevelEditorPopupLocalization.MicrophonePermissionDeniedPopupDescription);
                    break;
            }
        }

        protected override void OnPermissionRequestSucceeded()
        {
            UpdatePermissionStatus();
            
        #if UNITY_ANDROID && !UNITY_EDITOR
            RunMicrophoneCalibration();
        #else
            ResultCallback?.Invoke(new PermissionRequestResult(PermissionStatus));
        #endif
        }
        
#if UNITY_ANDROID && !UNITY_EDITOR
        private void RunMicrophoneCalibration()
        {
            if (!PermissionStatus.IsGranted())
            {
                ResultCallback?.Invoke(new PermissionRequestResult(PermissionStatus));
                return;
            }
            
            if (PlayerPrefs.HasKey(MICROPHONE_CALIBRATED_ID))
            {
                var microphoneUniqueId = PlayerPrefs.GetString(MICROPHONE_CALIBRATED_ID);
                OnCalibrationDone(microphoneUniqueId);
                return;
            }
            
            _pageModel.ShowLoadingOverlay("Calibrating microphone...");
            
            CoroutineSource.Instance.StartCoroutine(NatDeviceUtils.NatDeviceUtils.QueryMicrophones(OnCalibrationDone));

            void OnCalibrationDone(string microphoneUniqueId)
            {
                PlayerPrefs.SetString(MICROPHONE_CALIBRATED_ID, microphoneUniqueId);
                SetMicrophone();
                _pageModel.HideLoadingOverlay();
                
                ResultCallback?.Invoke(new PermissionRequestResult(PermissionStatus));
            }
        }
        
        private void SetMicrophone()
        {
            if (!PlayerPrefs.HasKey(MICROPHONE_CALIBRATED_ID)) return;

            var microphoneUniqueId = PlayerPrefs.GetString(MICROPHONE_CALIBRATED_ID);
                
            _voiceRecorder.SetMicrophone(microphoneUniqueId);
        }
#endif
    }
}