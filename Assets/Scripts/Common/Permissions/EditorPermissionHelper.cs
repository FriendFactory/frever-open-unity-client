#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;

namespace Common.Permissions
{
    internal sealed class EditorPermissionHelper : IPermissionsHelper
    {
        private readonly Dictionary<PermissionTarget, PermissionStatus> _permissionData;

        public EditorPermissionHelper(Dictionary<PermissionTarget, PermissionStatus> permissionData)
        {
            _permissionData = permissionData;
        }

        public bool HasPermission(PermissionTarget target)
        {
            return GetPermissionState(target).IsGranted();
        }

        public PermissionStatus GetPermissionState(PermissionTarget target)
        {
            AddDefaultIfNoData(target);
            return _permissionData[target];
        }

        public void RequestPermission(PermissionTarget target, Action onSuccess, Action<string> onFail = null)
        {
            AddDefaultIfNoData(target);
            ShowPermissionRequestDialog(target, onSuccess, onFail);
        }

        public void OpenNativeAppPermissionMenu()
        {
            EditorUtility.DisplayDialog("Opening Native Access Control Page Request", "It simulates granting camera and photo permission for the app", "Ok");
            GrantPermission(PermissionTarget.Camera);
            GrantPermission(PermissionTarget.Microphone);
            GrantPermission(PermissionTarget.Contacts);
            GrantPermission(PermissionTarget.Notifications);
        }

        private void AddDefaultIfNoData(PermissionTarget target)
        {
            if (_permissionData.ContainsKey(target)) return;
            _permissionData.Add(target, PermissionStatus.NotDetermined);
        }

        private void GrantPermission(PermissionTarget target)
        {
            _permissionData[target] = PermissionStatus.Authorized;
        }

        private void ShowPermissionRequestDialog(PermissionTarget target, Action onSuccess, Action<string> onFail = null)
        {
            var title = "Permission Dialog";
            var message = $"Frever would like to access your {target}";
            var okMessage = "Ok";
            var cancelMessage = "Don't allow";
            
            var result = EditorUtility.DisplayDialog(title, message, okMessage, cancelMessage);

            if (result)
            {
                _permissionData[target] = PermissionStatus.Authorized;
                onSuccess?.Invoke();
                return;
            }
            
            _permissionData[target] = PermissionStatus.Denied;
            onFail?.Invoke($"Failed to request permission # {target}");
        }
    }
}
#endif