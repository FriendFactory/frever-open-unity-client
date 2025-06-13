using Modules.LevelManaging.Editing.EventSaving;
using Modules.LevelManaging.GifPreview.Core;
using UIManaging.Common;
using UIManaging.Pages.LevelEditor.Ui.RecordingButton;
using Zenject;

namespace Installers
{
    internal static class LevelServicesBinder
    {
        public static void BindLevelServices(this DiContainer container)
        {
            container.Bind<EventSaver>().AsSingle();
            container.Bind<ILevelPreviewCapture>().To<LevelPreviewCapture>().AsSingle();
            container.BindInterfacesAndSelfTo<EventRecordingService>().AsSingle();
            container.BindInterfacesAndSelfTo<EventRecordingStateController>().AsSingle();
            container.BindInterfacesAndSelfTo<RemixLevelSetup>().AsSingle();
        }
    }
}
