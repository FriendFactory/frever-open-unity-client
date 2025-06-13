using Modules.AssetsManaging.Loaders;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.VideoMessage;
using UIManaging.Pages.VideoMessage.Emojis;
using UIManaging.Pages.VideoMessage.SetLocations;
using Zenject;

namespace Installers
{
    internal static class VideoMessagePageServicesBinder
    {
        public static void BindVideoMessagePageServices(this DiContainer container)
        {
            container.BindInterfacesAndSelfTo<EmotionsProvider>().AsSingle();
            container.BindInterfacesAndSelfTo<SetLocationsProvider>().AsSingle();
            container.BindInterfacesAndSelfTo<LevelForVideoMessageProvider>().AsSingle();
            container.BindInterfacesAndSelfTo<PictureInPictureCameraControl>().AsSingle();
            container.BindInterfacesAndSelfTo<VideoMessagePageGesturesControl>().AsSingle();
            container.BindInterfacesAndSelfTo<SetLocationBackgroundListProvider>().AsSingle().WithArguments(Common.Constants.VideoMessage.BACKGROUNDS_PAGE_SIZE);
            container.BindInterfacesAndSelfTo<SetLocationBackgroundThumbnailProvider>().AsSingle();
            container.BindInterfacesAndSelfTo<SetLocationBackgroundInMemoryCache>().AsSingle();
        }
    }
}