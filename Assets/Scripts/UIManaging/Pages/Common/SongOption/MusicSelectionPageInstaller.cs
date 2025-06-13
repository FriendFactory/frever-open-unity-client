using UIManaging.Pages.Common.FavoriteSounds;
using UIManaging.Pages.Common.SongOption.Amplitude;
using UIManaging.Pages.Common.SongOption.Common;
using UIManaging.Pages.Common.SongOption.MusicCue;
using UIManaging.Pages.Common.SongOption.MusicLicense;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.SongOption
{
    internal class MusicSelectionPageInstaller: MonoInstaller
    {
        [SerializeField] private MusicSelectionStateController _musicSelectionStateController;
        [SerializeField] private MusicCueScreen _musicCueScreen;
        [SerializeField] private MusicLicenseTypePanel _musicLicenseTypePanel;
        
        public override void InstallBindings()
        {
            Container.Bind<MusicSelectionStateController>().FromInstance(_musicSelectionStateController).AsCached();
            Container.Bind<MusicCueScreen>().FromInstance(_musicCueScreen).AsCached();
            Container.Bind<MusicSelectionPageModel>().AsCached();
            Container.Bind<MusicPlayerController>().AsCached();
            // TODO: keep persistent across LE an PiP scenes or during video creation context in general
            Container.BindInterfacesAndSelfTo<SoundsFavoriteStatusCache>().AsCached();
            Container.BindInterfacesAndSelfTo<MusicDataProvider>().AsCached();
            Container.BindInterfacesAndSelfTo<MusicLicenseManager>()
                     .AsCached()
                     .WithArgumentsExplicit(new[] { new TypeValuePair(typeof(MusicLicenseTypePanel), _musicLicenseTypePanel) });
            Container.BindInterfacesAndSelfTo<MusicSelectedAmplitudeEventSignalEmitter>().AsCached();
        }
    }
}