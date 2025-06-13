using Common;
using Common.ApplicationCore;
using Common.Services;
using Modules.TempSaves.Manager;
using UIManaging.Pages.Common.VideoManagement;
using Zenject;

namespace Installers
{
    internal static class MiscellaneousServicesBinder
    {
        public static void BindMiscellaneousServices(this DiContainer container, SessionInfo sessionInfo)
        {
            container.Bind<TempFileManager>().AsSingle();
            container.BindInterfacesAndSelfTo<SessionInfo>().FromInstance(sessionInfo).AsSingle();
            container.Bind<TrendingManager>().AsSingle();
            container.BindInterfacesAndSelfTo<TemplateManagingService>().AsSingle();
            container.BindInterfacesAndSelfTo<UserTaskProvider>().AsSingle();
            container.Bind<StopWatchProvider>().AsSingle();
            container.BindInterfacesAndSelfTo<AppEventsSource>().FromNewComponentOnNewGameObject().WithGameObjectName(nameof(AppEventsSource)).AsSingle();
        }
    }
}