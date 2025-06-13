using Modules.UniverseManaging;
using Zenject;

namespace Installers
{
    internal static class UniverseServicesBinder
    {
        public static void BindUniverseServices(this DiContainer container)
        {
            container.BindInterfacesAndSelfTo<UniverseManager>().AsSingle();
        }
    }
}