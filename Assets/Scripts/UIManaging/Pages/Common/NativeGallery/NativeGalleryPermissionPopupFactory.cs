using System;
using UIManaging.Localization;
using UIManaging.PopupSystem.Configurations;
using PermissionType = NativeGallery.PermissionType;
using MediaType = NativeGallery.MediaType;

namespace UIManaging.Pages.Common.NativeGalleryManagement
{
    internal interface INativeGalleryPermissionPopupFactory
    {
        PopupConfiguration CreatePopupConfiguration(PermissionType permissionType, Action onSuccess = null, Action onFail = null);
    }

    public sealed class NativeGalleryPermissionPopupFactory : INativeGalleryPermissionPopupFactory
    {
        private readonly MediaPermissionPopupConfigurationBuilder _popupConfigurationBuilder;
        private readonly NativeGalleryPermissionPopupLocalization _localization;
        
        public NativeGalleryPermissionPopupFactory(NativeGalleryPermissionPopupLocalization localization)
        {
            _localization = localization;
            _popupConfigurationBuilder = new MediaPermissionPopupConfigurationBuilder();
        }

        public PopupConfiguration CreatePopupConfiguration(PermissionType permissionType, Action onSuccess = null, Action onFail = null)
        {
            switch (permissionType)
            {
                case PermissionType.Read:
                    return CreateReadPopupConfiguration(onSuccess, onFail);
                case PermissionType.Write:
                    return CreateWritePopupConfiguration(onSuccess, onFail);
                default:
                    throw new ArgumentOutOfRangeException(nameof(permissionType), permissionType, null);
            }
        }

        private PopupConfiguration CreateWritePopupConfiguration(Action onSuccess, Action onFail)
        {
            return _popupConfigurationBuilder.WithTitleAndDescription(_localization.WriteAccessTitle, _localization.WriteAccessDescription,
                                                                      _localization.AllowButtonText, _localization.SettingsButtonText)
                                             .WithPermissionType(PermissionType.Write, MediaType.Video)
                                             .WithCallbacks(onSuccess, onFail)
                                             .Build();
            
        }
        
        private PopupConfiguration CreateReadPopupConfiguration(Action onSuccess, Action onFail)
        {
            return _popupConfigurationBuilder.WithTitleAndDescription(_localization.ReadAccessTitle, _localization.ReadAccessDescription,
                                                                      _localization.AllowButtonText, _localization.SettingsButtonText)
                                             .WithPermissionType(PermissionType.Read, MediaType.Video)
                                             .WithCallbacks(onSuccess, onFail)
                                             .Build();
        }
    }
}