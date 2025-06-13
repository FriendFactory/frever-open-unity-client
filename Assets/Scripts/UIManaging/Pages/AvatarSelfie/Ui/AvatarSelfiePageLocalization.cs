using I2.Loc;
using UnityEngine;

namespace UIManaging.Pages.AvatarSelfie.Ui
{
    public class AvatarSelfiePageLocalization : MonoBehaviour
    {
        [SerializeField]
        private LocalizedString _centerFaceMessage;
        [SerializeField]
        private LocalizedString _welcomePopupTitle;
        [SerializeField]
        private LocalizedString _welcomePopupMessage;
        [SerializeField]
        private LocalizedString _cameraPermissionPopupTitle;
        [SerializeField]
        private LocalizedString _openSettingsButton;
        [SerializeField]
        private LocalizedString _leaveButton;
        [SerializeField]
        private LocalizedString _serverDownPopupTitle;
        [SerializeField]
        private LocalizedString _serverDownPopupMessage;
        [SerializeField]
        private LocalizedString _serverDownPopupCloseButton;
    
        public string  CenterFaceMessage => _centerFaceMessage;
        public string  WelcomePopupMessage => _welcomePopupMessage;
        public string  WelcomePopupTitle => _welcomePopupTitle;
        public string  CameraPermissionPopupTitle => _cameraPermissionPopupTitle;
        public string  OpenSettingsButton => _openSettingsButton;
        public string  LeaveButton => _leaveButton;
        public string  ServerDownPopupTitle => _serverDownPopupTitle;
        public string  ServerDownPopupMessage => _serverDownPopupMessage;
        public string  ServerDownPopupCloseButton => _serverDownPopupCloseButton;
    }
}
