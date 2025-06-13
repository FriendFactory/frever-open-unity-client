using Modules.AppsFlyerManaging;
using Zenject;

namespace Installers
{
    internal static class AppsFlyerServicesBinder
    {
        public static void BindAppsFlyerServices(this DiContainer container, AppsFlyerManager manager)
        {
            container.Bind<AppsFlyerManager>().FromInstance(manager);
            container.BindInterfacesAndSelfTo<AppsFlyerService>().AsSingle();
        }
    }
}