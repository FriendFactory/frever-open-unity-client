using Common.Permissions;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;

namespace UIManaging.Pages.LevelEditor.Ui.Permissions
{
    [UsedImplicitly]
    internal class CameraPermissionHandler: PermissionHelperBase
    {
        protected override PermissionTarget PermissionTarget => PermissionTarget.Camera;
        
        private readonly PopupManager _popupManager;
        private readonly ILevelManager _levelManager;
        
        public CameraPermissionHandler(IPermissionsHelper permissionsHelper, PopupManager popupManager, ILevelManager levelManager, PopupManagerHelper popupManagerHelper): base(permissionsHelper, popupManagerHelper)
        {
            _popupManager = popupManager;
            _levelManager = levelManager;
        }

        protected override void RequestPermissionInternal()
        {
            switch (PermissionStatus)
            {
                case PermissionStatus.NotDetermined:
                    ShowPermissionRequestPopup();
                    return;
                case PermissionStatus.Denied:
                    RequestOpenAppPermissionMenuWithPopup(LevelEditorPopupLocalization.CameraPermissionDeniedPopupTitle,
                                                          LevelEditorPopupLocalization.CameraPermissionDeniedPopupDescription);
                    return;
            }
        }

        protected override void OnApplicationFocus(bool isFocused)
        {
            base.OnApplicationFocus(isFocused);
            
            if (IsStatusDetermined)
            {
                _levelManager.SetFaceTracking(IsPermissionGranted);
            }
        }
        
        private void ShowPermissionRequestPopup()
        {
            var contactPermissionPopupConfiguration = new DialogPopupConfiguration
            {
                PopupType = PopupType.CameraPermission,
                OnYes = AllowPermission, OnNo = DontAllow
            };

            _popupManager.SetupPopup(contactPermissionPopupConfiguration);
            _popupManager.ShowPopup(contactPermissionPopupConfiguration.PopupType);

            void AllowPermission()
            {
                _popupManager.ClosePopupByType(PopupType.CameraPermission);
                RequestTargetPermission();
            }

            void DontAllow()
            {
                ResultCallback?.Invoke(new PermissionRequestResult());
                _popupManager.ClosePopupByType(PopupType.CameraPermission);
            }
        }
    }
}