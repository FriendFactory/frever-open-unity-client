using Common;
using UIManaging.Pages.Common.SongOption.Common;
using Zenject;

namespace UIManaging.Pages.FavoriteSounds
{
    internal sealed class VideosBasedOnSoundPageInstaller: MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<MusicPlayerController>().AsSingle();
            Container.Bind<StopWatchProvider>().AsSingle();
        }
    }
}