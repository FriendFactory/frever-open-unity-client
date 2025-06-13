using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    public class LevelEditorPopupLocalization : ScriptableObject
    {
        [SerializeField] private LocalizedString _exitPopupOptionsTitle;
        [SerializeField] private LocalizedString _exitPopupDeleteStartOverOption;
        [SerializeField] private LocalizedString _exitPopupExitSaveDraftOption;
        [SerializeField] private LocalizedString _exitPopupExitOption;
        [SerializeField] private LocalizedString _exitPopupEraseChangesAndGoBackOption;
        
        [SerializeField] private LocalizedString _discardRecordingsPopupDescription;
        [SerializeField] private LocalizedString _discardRecordingsPopupDiscardButton;
        [SerializeField] private LocalizedString _discardRecordingsPopupCancelButton;
        
        [SerializeField] private LocalizedString _deleteClipPopupDescription;
        [SerializeField] private LocalizedString _deleteClipConfirmButton;
        [SerializeField] private LocalizedString _deleteClipCancelButton;
        
        [SerializeField] private LocalizedString _cameraPermissionDeniedPopupTitle;
        [SerializeField] private LocalizedString _cameraPermissionDeniedPopupDescription;
        [SerializeField] private LocalizedString _microphonePermissionDeniedPopupTitle;
        [SerializeField] private LocalizedString _microphonePermissionDeniedPopupDescription;
        [SerializeField] private LocalizedString _permissionDeniedPopupSettingsButton;
        [SerializeField] private LocalizedString _permissionDeniedPopupCancelButton;
        
        public string CameraPermissionDeniedPopupTitle => _cameraPermissionDeniedPopupTitle;
        public string CameraPermissionDeniedPopupDescription => _cameraPermissionDeniedPopupDescription;
        public string MicrophonePermissionDeniedPopupTitle => _microphonePermissionDeniedPopupTitle;
        public string MicrophonePermissionDeniedPopupDescription => _microphonePermissionDeniedPopupDescription;
        public string PermissionDeniedPopupSettingsButton => _permissionDeniedPopupSettingsButton;
        public string PermissionDeniedPopupCancelButton => _permissionDeniedPopupCancelButton;
        
        public string ExitPopupOptionsTitle => _exitPopupOptionsTitle;
        public string ExitPopupDeleteStartOverOption => _exitPopupDeleteStartOverOption;
        public string ExitPopupExitSaveDraftOption => _exitPopupExitSaveDraftOption;
        public string ExitPopupExitOption => _exitPopupExitOption;
        public string ExitPopupEraseChangesAndGoBackOption => _exitPopupEraseChangesAndGoBackOption;

        public string DiscardRecordingsPopupDescription => _discardRecordingsPopupDescription;
        public string DiscardRecordingsPopupDiscardButton => _discardRecordingsPopupDiscardButton;
        public string DiscardRecordingsPopupCancelButton => _discardRecordingsPopupCancelButton;

        public string DeleteClipPopupDescription => _deleteClipPopupDescription;
        public string DeleteClipConfirmButton => _deleteClipConfirmButton;
        public string DeleteClipCancelButton => _deleteClipCancelButton;
       
    }
}