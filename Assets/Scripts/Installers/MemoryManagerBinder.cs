using Modules.MemoryManaging;
using Zenject;

namespace Installers
{
    internal static class MemoryManagerBinder
    {
        private const int AVAILABLE_RAM_IN_EDITOR_MB = 900;
        
        public static void BindMemoryManager(this DiContainer container)
        {
            #if UNITY_EDITOR
            container.Bind<IMemoryManager>().To<StubMemoryManager>().AsSingle().WithArguments(AVAILABLE_RAM_IN_EDITOR_MB);
            #elif UNITY_IOS
            container.Bind<IMemoryManager>().To<MemoryManagerIOS>().AsSingle();
            #elif UNITY_ANDROID
            container.Bind<IMemoryManager>().To<MemoryManagerAndroid>().AsSingle();
            #endif
            
            container.BindInterfacesAndSelfTo<CacheManager>().AsSingle();
        }
    }
}