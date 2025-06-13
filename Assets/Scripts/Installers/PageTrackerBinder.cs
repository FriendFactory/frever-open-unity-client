using Zenject;
using Modules.PageLoadTracking;

namespace Installers
{
    public static class PageTrackerBinder
    {
        public static void BindPageLoadTrackers(this DiContainer container)
        {
            container.BindInterfacesAndSelfTo<LevelEditorPageLoadTimeTracker>().AsSingle();
            container.BindInterfacesAndSelfTo<PageLoadTimeTracker>().AsSingle();
            container.BindInterfacesAndSelfTo<UmaEditorPageLoadTimeTracker>().AsSingle();
        }
    }
}