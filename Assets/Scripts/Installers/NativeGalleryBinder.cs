using UIManaging.Pages.Common.NativeGalleryManagement;
using Zenject;

namespace Installers
{
    internal static class NativeGalleryBinder
    {
        public static void BindNativeGallery(this DiContainer container)
        {
            #if !UNITY_EDITOR
            container.BindInterfacesAndSelfTo<NativeGalleryService>().AsSingle();
            #else
            container.BindInterfacesAndSelfTo<EditorNativeGallery>().AsSingle();
            #endif
        }
    }
}
