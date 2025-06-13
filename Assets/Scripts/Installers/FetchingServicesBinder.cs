using AppStart;
using Modules.AssetsStoraging.Core;
using Zenject;

namespace Installers
{
    internal static class FetchingServicesBinder
    {
        public static void BindFetchingServices(this DiContainer container, FetcherConfig fetcherConfig)
        {
            container.Bind<CriticalDataFetcher>().FromInstance(AppEntryContext.CriticalDataFetcher);
            container.BindInterfacesAndSelfTo<DataFetcher>().AsSingle();
            container.BindInterfacesAndSelfTo<WardrobeCategoriesProvider>().AsSingle();
            container.Bind<FetcherConfig>().FromInstance(fetcherConfig);
        }
    }
}
