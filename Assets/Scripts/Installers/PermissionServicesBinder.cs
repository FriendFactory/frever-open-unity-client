using System.Collections.Generic;
using Common.Permissions;
using Modules.Notifications;
using Zenject;

namespace Installers
{
    internal static class PermissionServicesBinder
    {
        public static void BindPermissionServices(this DiContainer container)
        {
            #if UNITY_IOS && !UNITY_EDITOR
            container.BindInterfacesAndSelfTo<iOSPermissionsHelper>().AsSingle();
            #elif UNITY_ANDROID && !UNITY_EDITOR
            container.BindInterfacesAndSelfTo<AndroidPermissionsHelper>().AsSingle();
            #elif UNITY_EDITOR
            var editorSetup = new Dictionary<PermissionTarget, PermissionStatus>()
            {
                { PermissionTarget.Camera, PermissionStatus.Authorized },
                { PermissionTarget.Microphone, PermissionStatus.Authorized },
                { PermissionTarget.Contacts, PermissionStatus.Authorized },
                { PermissionTarget.Notifications, PermissionStatus.NotDetermined },
            };
            container.BindInterfacesAndSelfTo<EditorPermissionHelper>().AsSingle().WithArguments(editorSetup);
            #endif

            container.BindInterfacesAndSelfTo<NotificationsPermissionHandler>().AsSingle();
        }
    }
}