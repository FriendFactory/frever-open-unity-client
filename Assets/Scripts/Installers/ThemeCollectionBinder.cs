using Modules.ThemeCollection;
using Zenject;

namespace Installers
{
    public static class ThemeCollectionBinder
    {
        public static void BindThemeCollectionService(this DiContainer container)
        {
            container.BindInterfacesAndSelfTo<ThemeCollectionService>().AsSingle();
        }
    }
}