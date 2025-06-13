using Modules.RenderingPipelineManagement;
using Zenject;

#if UNITY_IOS || UNITY_EDITOR
    using Modules.RenderingPipelineManagement.iOS;
#elif UNITY_ANDROID
    using Modules.RenderingPipelineManagement.Android;
#endif

namespace Installers
{
    internal static class RenderingPipelineManagerBinder
    {
        public static void BindRenderingPipelineManager(this DiContainer container)
        {
            #if UNITY_IOS || UNITY_EDITOR
                container.Bind<IRenderingPipelineManager>().To<iOSRenderingPipelineManager>().AsSingle();
            #elif UNITY_ANDROID
			    container.Bind<IRenderingPipelineManager>().To<AndroidRenderingPipelineManager>().AsSingle();
            #endif
        } 
    }
}