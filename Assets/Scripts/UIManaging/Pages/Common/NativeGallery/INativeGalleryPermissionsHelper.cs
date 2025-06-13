using System;

namespace UIManaging.Pages.Common.NativeGalleryManagement
{
    public interface INativeGalleryPermissionsHelper
    {
        bool IsPermissionGranted(NativeGallery.PermissionType permissionType, NativeGallery.MediaType mediaType);
        void RequestPermission(NativeGallery.PermissionType permissionType, Action onSuccess = null, Action onFail = null);
    }
}