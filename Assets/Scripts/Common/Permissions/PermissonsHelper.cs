using System;

namespace Common.Permissions
{
    public interface IPermissionsHelper
    {
        bool HasPermission(PermissionTarget target);
        PermissionStatus GetPermissionState(PermissionTarget target);
        void RequestPermission(PermissionTarget target, Action onSuccess, Action<string> onFail = null);
        void OpenNativeAppPermissionMenu();
    }
}
