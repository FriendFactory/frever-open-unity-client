using System;
using Common.Permissions;
using StansAssets.Foundation.Async;
using UIManaging.Localization;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.Permissions
{
    public abstract class PermissionHelperBase: IInitializable, IDisposable
    {
        [Inject] protected LevelEditorPopupLocalization LevelEditorPopupLocalization;
        
        protected abstract PermissionTarget PermissionTarget { get; }
        
        public bool IsPermissionGranted => PermissionStatus.IsGranted();
        public bool IsStatusDetermined => PermissionStatus.IsDetermined();

        protected readonly IPermissionsHelper PermissionsHelper;
        protected readonly PopupManagerHelper PopupManagerHelper;

        protected PermissionStatus PermissionStatus
        {
            get => _permissionStatus;
            private set
            {
                if (_permissionStatus == value) return;

                _permissionStatus = value;

                PermissionStatusChanged?.Invoke(_permissionStatus);
            }
        }
        
        protected Action<PermissionRequestResult> ResultCallback;
        private PermissionStatus _permissionStatus;
        
        public event Action<PermissionStatus> PermissionStatusChanged;
        public event Action OpenAppSettingsRequestCanceled;

        protected PermissionHelperBase(IPermissionsHelper permissionsHelper, PopupManagerHelper popupManagerHelper)
        {
            PermissionsHelper = permissionsHelper;
            PopupManagerHelper = popupManagerHelper;
        }
        
        public virtual void Initialize()
        {
            UpdatePermissionStatus();
            
            MonoBehaviourCallback.ApplicationOnFocus += OnApplicationFocus;
        }
        
        public void Dispose()
        {
            MonoBehaviourCallback.ApplicationOnFocus -= OnApplicationFocus;
        }
        
        public void RequestPermission(Action<PermissionRequestResult> resultCallback = null)
        {
            ResultCallback = resultCallback;
            RequestPermissionInternal();
        }

        protected abstract void RequestPermissionInternal();
        

        protected virtual void OnApplicationFocus(bool isFocused)
        {
            if (!isFocused) return; 
            
            UpdatePermissionStatus();
        }

        protected virtual void OnPermissionRequestSucceeded()
        {
            UpdatePermissionStatus();

            ResultCallback?.Invoke(new PermissionRequestResult(PermissionStatus));
        }

        protected virtual void OnPermissionRequestFailed(string error)
        {
            UpdatePermissionStatus();

            ResultCallback?.Invoke(new PermissionRequestResult(error));
        }

        protected void RequestTargetPermission()
        {
            PermissionsHelper.RequestPermission(PermissionTarget, OnPermissionRequestSucceeded, OnPermissionRequestFailed);
        }

        protected void UpdatePermissionStatus()
        {
            PermissionStatus = PermissionsHelper.GetPermissionState(PermissionTarget);
        }

        protected void RequestOpenAppPermissionMenuWithPopup(string title, string description)
        {
            PopupManagerHelper.ShowDialogPopup(title, 
                                               description, 
                                               LevelEditorPopupLocalization.PermissionDeniedPopupCancelButton, 
                                               OnNo, 
                                               LevelEditorPopupLocalization.PermissionDeniedPopupSettingsButton,
                                               OnYes, 
                                               false);
            
            void OnYes()
            {
                PermissionsHelper.OpenNativeAppPermissionMenu();
            }
            
            void OnNo()
            {
                OpenAppSettingsRequestCanceled?.Invoke();
            }
        }
    }
}