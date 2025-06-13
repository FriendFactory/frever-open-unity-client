using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using OneSignalSDK;
using SA.Android.App;
using SA.Android.Content;
using SA.Android.Content.Pm;
using SA.Android.Manifest;
using SA.Android.Provider;
using UnityEngine;

namespace Common.Permissions
{
    [UsedImplicitly]
    public class AndroidPermissionsHelper : IPermissionsHelper
    {
        private static readonly string MICROPHONE_PERMISSION_REQUESTED_ONCE_ID = $"{Application.identifier}.UserRequestedMicrophonePermissionOnce";
        private static readonly string CAMERA_PERMISSION_REQUESTED_ONCE_ID = $"{Application.identifier}.UserRequestedCameraPermissionOnce";
        private static readonly string CONTACTS_PERMISSION_REQUESTED_ONCE_ID = $"{Application.identifier}.UserRequestedContactsPermissionOnce";
        private static readonly string NOTIFICATION_PERMISSION_REQUESTED_ONCE_ID = $"{Application.identifier}.UserRequestedNotificationsPermissionOnce";
        
        private readonly IReadOnlyDictionary<PermissionTarget, AMM_ManifestPermission> _targetsMap =
            new Dictionary<PermissionTarget, AMM_ManifestPermission>()
            {
                { PermissionTarget.Camera, AMM_ManifestPermission.CAMERA },
                { PermissionTarget.Microphone, AMM_ManifestPermission.RECORD_AUDIO },
                { PermissionTarget.Contacts, AMM_ManifestPermission.READ_CONTACTS },
            };

        private readonly IReadOnlyDictionary<AN_PackageManager.PermissionState, PermissionStatus> _statusMap =
            new Dictionary<AN_PackageManager.PermissionState, PermissionStatus>()
            {
                { AN_PackageManager.PermissionState.Granted, PermissionStatus.Authorized },
                { AN_PackageManager.PermissionState.Denied, PermissionStatus.Denied },
            };

        private readonly IReadOnlyDictionary<NativeGallery.Permission, PermissionStatus> _nativeGalleryStatusMap =
            new Dictionary<NativeGallery.Permission, PermissionStatus>
            {
                { NativeGallery.Permission.Granted, PermissionStatus.Authorized },
                { NativeGallery.Permission.Denied, PermissionStatus.Denied },
                { NativeGallery.Permission.ShouldAsk, PermissionStatus.NotDetermined },
            };

        private readonly IReadOnlyDictionary<NotificationPermission, PermissionStatus> _notificationsStatusMap =
            new Dictionary<NotificationPermission, PermissionStatus>()
            {
                {NotificationPermission.Authorized, PermissionStatus.Authorized},
                {NotificationPermission.Denied, PermissionStatus.Denied},
                {NotificationPermission.NotDetermined, PermissionStatus.NotDetermined},
            };

        private readonly IReadOnlyDictionary<PermissionTarget, string> _targetIdsMap =
            new Dictionary<PermissionTarget, string>()
            {
                { PermissionTarget.Camera, CAMERA_PERMISSION_REQUESTED_ONCE_ID },
                { PermissionTarget.Microphone, MICROPHONE_PERMISSION_REQUESTED_ONCE_ID},
                { PermissionTarget.Contacts, CONTACTS_PERMISSION_REQUESTED_ONCE_ID },
                { PermissionTarget.Notifications, NOTIFICATION_PERMISSION_REQUESTED_ONCE_ID },
            };

        public bool HasPermission(PermissionTarget target)
        {
            return GetPermissionState(target).IsGranted();
        }
        
        public bool HasPhoneCallPermission()
        {
            return AN_PermissionsManager.CheckSelfPermission(AMM_ManifestPermission.READ_PHONE_STATE) 
                == AN_PackageManager.PermissionState.Granted;
        }

        public PermissionStatus GetPermissionState(PermissionTarget target)
        {
            if (!PlayerPrefs.HasKey(_targetIdsMap[target])) return PermissionStatus.NotDetermined;
            
            switch (target)
            {
                case PermissionTarget.Microphone:
                case PermissionTarget.Camera:
                case PermissionTarget.Contacts:
                    return GetPermissionStatus(target);
                case PermissionTarget.Notifications:
                    return GetNotificationsPermissionStatus();
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }

        public void RequestPermission(PermissionTarget target, Action onSuccess, Action<string> onFail = null)
        {
            if (!PlayerPrefs.HasKey(_targetIdsMap[target]))
            {
                PlayerPrefs.SetInt(_targetIdsMap[target], 1);
            }
            
            switch (target)
            {
                case PermissionTarget.Microphone:
                case PermissionTarget.Camera:
                case PermissionTarget.Contacts:
                    RequestManifestPermission(_targetsMap[target], onSuccess, onFail);
                    break;
                case PermissionTarget.Notifications:
                    RequestNotificationsPermission(onSuccess, onFail);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }

        public void OpenNativeAppPermissionMenu()
        {
            var uri = new Uri($"package:{Application.identifier}");
            var intent = new AN_Intent(AN_Settings.ACTION_APPLICATION_DETAILS_SETTINGS, uri);
            AN_MainActivity.Instance.StartActivity(intent);
        }
        
        private PermissionStatus GetPermissionStatus(PermissionTarget target)
        {
            var state = AN_PermissionsManager.CheckSelfPermission(_targetsMap[target]);

            return _statusMap[state];
        }

        public void RequestManifestPermission(AMM_ManifestPermission permission, Action onSuccess, Action<string> onFail)
        {
            AN_PermissionsUtility.TryToResolvePermission(permission, granted =>
            {
                if (granted)
                {
                    onSuccess?.Invoke();
                    return;
                }
                
                onFail?.Invoke($"[{GetType().Name}] Permission {permission} was not granted.");
            });
        }

        private PermissionStatus GetNativeGalleryPermissionStatus()
        {
            var status = NativeGallery.CheckPermission(NativeGallery.PermissionType.Write, NativeGallery.MediaType.Image | NativeGallery.MediaType.Video);
            return _nativeGalleryStatusMap[status];
        }

        private async void RequestNotificationsPermission(Action onSuccess, Action<string> onFail)
        {
            var response = await OneSignal.Default.PromptForPushNotificationsWithUserResponse();
            if (response == NotificationPermission.Authorized)
            {
                onSuccess?.Invoke();
            }
            else
            {
                onFail?.Invoke($"[{GetType().Name}] Access to permissions was not granted.");
            }
        }

        private PermissionStatus GetNotificationsPermissionStatus()
        {
            try
            {
                var status = OneSignal.Default.NotificationPermission;
                return _notificationsStatusMap[status];
            }
            catch (NullReferenceException ex)
            {
                Debug.LogWarning($"[{GetType().Name}] {ex.Message}");
                return PermissionStatus.NotDetermined;
            }
        }
    }
}