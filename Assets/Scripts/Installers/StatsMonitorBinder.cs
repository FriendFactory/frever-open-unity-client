using Tayx.Graphy;
using Zenject;

namespace Installers
{
    internal static class StatsMonitorBinder
    {
        public static void BindStatsMonitor(this DiContainer container, GraphyManager _graphyManagerPrefab)
        {
        #if STATS_ENABLED
            container.BindInterfacesAndSelfTo<GraphyManager>()
                     .FromComponentInNewPrefab(_graphyManagerPrefab)
                     .AsSingle()
                     .NonLazy();
        #endif
        }
    }
}