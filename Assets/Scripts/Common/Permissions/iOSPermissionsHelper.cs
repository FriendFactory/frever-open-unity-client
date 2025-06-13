using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using OneSignalSDK;
using SA.iOS.AVFoundation;
using SA.iOS.Contacts;
using SA.iOS.UIKit;
using static Common.Permissions.PermissionStatus;
using static Common.Permissions.PermissionTarget;

namespace Common.Permissions
{
    [UsedImplicitly]
    internal sealed class iOSPermissionsHelper : IPermissionsHelper
    {
        private readonly IReadOnlyDictionary<PermissionTarget, ISN_AVMediaType> _targetsMap =
            new Dictionary<PermissionTarget, ISN_AVMediaType>()
            {
                {Camera, ISN_AVMediaType.Video},
                {Microphone, ISN_AVMediaType.Audio},
            };

        private readonly IReadOnlyDictionary<ISN_AVAuthorizationStatus, PermissionStatus> _captureDeviceStatusMap =
            new Dictionary<ISN_AVAuthorizationStatus, PermissionStatus>()
            {
                {ISN_AVAuthorizationStatus.Authorized, Authorized},
                {ISN_AVAuthorizationStatus.Denied, Denied},
                {ISN_AVAuthorizationStatus.Restricted, Restricted},
                {ISN_AVAuthorizationStatus.NotDetermined, NotDetermined}
            };
        
        private readonly IReadOnlyDictionary<ISN_CNAuthorizationStatus, PermissionStatus> _contactStoreStatusMAp =
            new Dictionary<ISN_CNAuthorizationStatus, PermissionStatus>()
            {
                {ISN_CNAuthorizationStatus.Authorized, Authorized},
                {ISN_CNAuthorizationStatus.Denied, Denied},
                {ISN_CNAuthorizationStatus.Restricted, Restricted},
                {ISN_CNAuthorizationStatus.NotDetermined, NotDetermined}
            };
        
        private readonly IReadOnlyDictionary<NotificationPermission, PermissionStatus> _notificationsStatusMap =
            new Dictionary<NotificationPermission, PermissionStatus>()
            {
                {NotificationPermission.Authorized, Authorized},
                {NotificationPermission.Denied, Denied},
                {NotificationPermission.NotDetermined, NotDetermined},
            };
        
        public bool HasPermission(PermissionTarget target)
        {
            return GetPermissionState(target).IsGranted();
        }

        public PermissionStatus GetPermissionState(PermissionTarget target)
        {
            switch (target)
            {
                case Camera:
                case Microphone:
                    return GetCaptureDevicePermissionStatus(target);
                case Contacts:
                    return GetContactStorePermissionStatus();
                case Notifications:
                    return GetNotificationsPermissionStatus();
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }

        public void RequestPermission(PermissionTarget target, Action onSuccess, Action<string> onFail)
        {
            switch (target)
            {
                case Camera:
                case Microphone:
                    RequestCaptureDevicePermissionIOS(_targetsMap[target], onSuccess, onFail);
                    break;
                case Contacts:
                    RequestContactStorePermission(onSuccess, onFail);
                    break;
                case Notifications:
                    RequestNotificationsPermission(onSuccess, onFail);
                    break;
            }
        }

        public void OpenNativeAppPermissionMenu()
        {
            var url = ISN_UIApplication.OpenSettingsURLString;
            ISN_UIApplication.OpenURL(url);
        }

        private void RequestCaptureDevicePermissionIOS(ISN_AVMediaType mediaType, Action onSuccess, Action<string> onFail)
        {
            ISN_AVCaptureDevice.RequestAccess(mediaType, result =>
            {
                if (result == ISN_AVAuthorizationStatus.Authorized)
                {
                    onSuccess?.Invoke();
                }
                else
                {
                    onFail?.Invoke("Access to capture device was not authorized");
                }
            });
        }

        private void RequestContactStorePermission(Action onSuccess, Action<string> onFail)
        {
            ISN_CNContactStore.RequestAccess(ISN_CNEntityType.Contacts, result =>
            {
                if (result.IsSucceeded)
                {
                    onSuccess?.Invoke();
                }
                else
                {
                    onFail?.Invoke($"Failed to get access to contacts. Reason: {result.Error}");
                }
            });
        }
        
        private PermissionStatus GetCaptureDevicePermissionStatus(PermissionTarget target)
        {
            var status = ISN_AVCaptureDevice.GetAuthorizationStatus(_targetsMap[target]);
            return _captureDeviceStatusMap[status];
        }
        
        private PermissionStatus GetContactStorePermissionStatus()
        {
            var status = ISN_CNContactStore.GetAuthorizationStatus(ISN_CNEntityType.Contacts);
            return _contactStoreStatusMAp[status];
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
            var status = OneSignal.Default.NotificationPermission;
            return _notificationsStatusMap[status];
        }
    }
}