using Modules.LevelManaging.Editing.ThumbnailCreator;
using UIManaging.Pages.Common.Helpers;
using UIManaging.Pages.LevelEditor.Ui;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor;
using Zenject;

namespace Installers
{
    internal static class ThumbnailServicesBinder
    {
        public static void BindThumbnailServices(this DiContainer container)
        {
            container.Bind<CharacterThumbnailsDownloader>().AsSingle();
            container.Bind<EventThumbnailsCreatorManager>().AsSingle();
            container.Bind<EventThumbnailCapture>().AsSingle();
            container.BindInterfacesAndSelfTo<MovementTypeThumbnailsProvider>().AsSingle();
        }
    }
}
