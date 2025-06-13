using Common;
using Modules.FaceAndVoice.Face.Facade;
using UnityEngine.XR.ARFoundation;
using Zenject;
using UnityEngine;

namespace Installers
{
    internal static class ArServicesBinder
    {
        public static void BindARServices(this DiContainer container, ARFaceManager arFaceManager, ARCameraManager arCameraManager, ARSession arSession)
        {
            container.BindArSessionManager();
            container.BindInterfacesAndSelfTo<ARSession>().FromInstance(arSession).AsSingle();
            container.BindInterfacesAndSelfTo<ARFaceManager>().FromInstance(arFaceManager).AsSingle();
            container.BindInterfacesAndSelfTo<ARCameraManager>().FromInstance(arCameraManager).AsSingle();
        }

        private static void BindArSessionManager(this DiContainer container)
        {
            if (DeviceInformationHelper.DeviceUsesARFoundations())
            {
                container.BindInterfacesAndSelfTo<ARFoundationSessionManager>().AsSingle();
            }
            else
            {
                container.BindInterfacesAndSelfTo<NoTrueDepthARSessionManager>().AsSingle();
            }
        }
    }
}