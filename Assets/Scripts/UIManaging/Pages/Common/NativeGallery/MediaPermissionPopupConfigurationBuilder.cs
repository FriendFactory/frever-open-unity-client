using System;
using UIManaging.PopupSystem.Configurations;

namespace UIManaging.Pages.Common.NativeGalleryManagement
{
    internal sealed class MediaPermissionPopupConfigurationBuilder
    {
        private string Title { get; set; }
        private string Description { get; set; }
        private string AllowButtonText { get; set; }
        private string SettingsButtonText { get; set; }
        private NativeGallery.PermissionType PermissionType { get; set; }
        private NativeGallery.MediaType MediaType { get; set; }
        private Action OnSuccess { get; set; }
        private Action OnFail { get; set; }

        public MediaPermissionPopupConfigurationBuilder WithTitleAndDescription(string title, string description, string allowButtonText, string settingsButtonText)
        {
            Title = title;
            Description = description;
            AllowButtonText = allowButtonText;
            SettingsButtonText = settingsButtonText;
            return this;
        }

        public MediaPermissionPopupConfigurationBuilder WithPermissionType(NativeGallery.PermissionType permissionType,
            NativeGallery.MediaType mediaType)
        {
            PermissionType = permissionType;
            MediaType = mediaType;

            return this;
        }

        public MediaPermissionPopupConfigurationBuilder WithCallbacks(Action onSuccess, Action onFail)
        {
            OnSuccess = onSuccess;
            OnFail = onFail;

            return this;
        }

        public PopupConfiguration Build()
        {
            var permission = NativeGallery.CheckPermission(PermissionType, MediaType);
            var yesButtonText = permission == NativeGallery.Permission.ShouldAsk
                    ? AllowButtonText
                    : SettingsButtonText;
            PopupConfiguration config = new DialogPopupConfiguration
            {
                PopupType = PopupType.NativeGalleryPermission,
                Title = Title,
                Description = Description,
                YesButtonText = yesButtonText,
                OnYes = RequestPermission,
                OnNo = OnNo
            };

            void OnNo() => OnFail?.Invoke();

            return config;
        }

        private void RequestPermission()
        {
            var permission = NativeGallery.CheckPermission(PermissionType, MediaType);
            switch (permission)
            {
                case NativeGallery.Permission.ShouldAsk:
                    OpenSystemPopup();
                    break;
                case NativeGallery.Permission.Denied:
                    OpenSettings();
                    break;
            }

            void OpenSystemPopup()
            {
                var result = NativeGallery.RequestPermission(PermissionType, MediaType);
                if (result == NativeGallery.Permission.Granted)
                {
                    OnSuccess?.Invoke();
                }
                else
                {
                    OnFail?.Invoke();
                }
            }

            void OpenSettings()
            {
                if (NativeGallery.CanOpenSettings()) NativeGallery.OpenSettings();
                OnFail?.Invoke();
            }
        }
    }
}