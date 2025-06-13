using System;
using JetBrains.Annotations;
using UIManaging.Localization;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using Zenject;
using PermissionType = NativeGallery.PermissionType;
using MediaType = NativeGallery.MediaType;

namespace UIManaging.Pages.Common.NativeGalleryManagement
{
    [UsedImplicitly]
    internal class NativeGalleryService : INativeGallery, INativeGalleryPermissionsHelper
    {
        // NativeGallery requests image and video permission regardless of MediaType
        private const MediaType READ_PERMISSION_MEDIA_TYPE = MediaType.Video;

        [Inject] private PopupManager _popupManager;

        private readonly INativeGalleryPermissionPopupFactory _permissionPopupFactory;

        [Inject]
        public NativeGalleryService(NativeGalleryPermissionPopupLocalization localization)
        {
            _permissionPopupFactory = new NativeGalleryPermissionPopupFactory(localization);
        }
        
        //---------------------------------------------------------------------
        // INativeGallery implementation 
        //---------------------------------------------------------------------

        public void GetMixedMediaFromGallery(NativeGallery.MediaPickCallback callback, MediaType mediaTypes, string title = "")
        {
            TryGetMedia(OnSuccess, OnFail);

            void OnSuccess()
            {
                NativeGallery.GetMixedMediaFromGallery(callback, mediaTypes, title);
            }

            void OnFail() => callback?.Invoke(null);
        }

        public void GetVideoFromGallery(NativeGallery.MediaPickCallback callback, string title = "")
        {
            TryGetMedia(OnSuccess, OnFail);

            void OnSuccess()
            {
                NativeGallery.GetVideoFromGallery(callback, title);
            }

            void OnFail() => callback?.Invoke(null);
        }

        public void SaveVideoWithCurrentDateToGallery(byte[] mediaBytes, string album, string filename, Action<bool, string> callback = null)
        {
            TryWriteMedia(OnSuccess, OnFail);
            
            void OnSuccess()
            {
                NativeGallery.SaveVideoToGallery(mediaBytes, album, filename, (success, path) => callback?.Invoke(success, path));
            }

            void OnFail()
            {
                callback?.Invoke(false, string.Empty);
            }
        }

        public void SaveVideoToGallery(byte[] mediaBytes, string album, string filename, Action<bool, string> callback = null)
        {
            TryWriteMedia(OnSuccess, OnFail);
            
            void OnSuccess()
            {
                NativeGallery.SaveVideoToGallery(mediaBytes, album, filename, (success, path) => callback?.Invoke(success, path));
            }
            
            void OnFail()
            {
                callback?.Invoke(false, string.Empty);
            }
        }

        public void SaveVideoToGallery(string existingMediaPath, string album, string filename, Action<bool, string> callback = null)
        {
            TryWriteMedia(OnSuccess, OnFail);

            void OnSuccess()
            {
                NativeGallery.SaveVideoToGallery(existingMediaPath, album, filename,
                                                 (success, path) => callback?.Invoke(true, path));
            }
            
            void OnFail()
            {
                callback?.Invoke(false, string.Empty);
            }
        }

        public Texture2D LoadImageAtPath(string imagePath, int maxSize = -1, bool markTextureNonReadable = true, bool generateMipmaps = true, bool linearColorSpace = false)
        {
            return NativeGallery.LoadImageAtPath(imagePath, maxSize, markTextureNonReadable, generateMipmaps, linearColorSpace);
        }
        
        public Texture2D GetVideoThumbnail(string videoPath)
        {
            return NativeGallery.GetVideoThumbnail(videoPath);
        }

        public NativeGallery.VideoProperties GetVideoInfo(string videoPath)
        {
            return NativeGallery.GetVideoProperties(videoPath);
        }
        
        //---------------------------------------------------------------------
        // INativeGalleryPermissionHelper implementation 
        //---------------------------------------------------------------------

        public bool IsPermissionGranted(PermissionType permissionType, MediaType mediaType)
        {
            var permission = NativeGallery.CheckPermission(permissionType, mediaType);

            return permission == NativeGallery.Permission.Granted;
        }

        public void RequestPermission(PermissionType permissionType, Action onSuccess = null, Action onFail = null)
        {
            var config = _permissionPopupFactory.CreatePopupConfiguration(permissionType, onSuccess, onFail);
            
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(PopupType.NativeGalleryPermission);
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private bool IsWritePermissionGranted() => IsPermissionGranted(PermissionType.Write, MediaType.Video);
        private bool IsReadPermissionGranted() => IsPermissionGranted(PermissionType.Read, READ_PERMISSION_MEDIA_TYPE);

        private void TryGetMedia(Action onSuccess, Action onFail)
        {
            if (IsReadPermissionGranted())
            {
                onSuccess?.Invoke();
                return;
            }

            var config = _permissionPopupFactory.CreatePopupConfiguration(PermissionType.Read, OnSuccess, OnFail);
            
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(PopupType.NativeGalleryPermission);

            void OnSuccess()
            {
                onSuccess.Invoke();
            }

            void OnFail() => onFail?.Invoke();
        }

        private void TryWriteMedia(Action onSuccess, Action onFail)
        {
            if (IsWritePermissionGranted())
            {
                onSuccess?.Invoke();
                return;
            }

            var config = _permissionPopupFactory.CreatePopupConfiguration(PermissionType.Write, OnSuccess, OnFail);
            
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(PopupType.NativeGalleryPermission);

            void OnSuccess()
            {
                onSuccess.Invoke();
            }

            void OnFail() => onFail?.Invoke();
        }
    }
}