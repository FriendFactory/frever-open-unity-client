using Bridge.Models.Common;
using JetBrains.Annotations;
using Modules.Amplitude.Signals;
using UIManaging.Pages.Common.SongOption.Common;
using Zenject;

namespace UIManaging.Pages.Common.SongOption.Amplitude
{
    [UsedImplicitly]
    internal sealed class MusicSelectedAmplitudeEventSignalEmitter: BaseAmplitudeEventSignalEmitter
    {
        private readonly MusicPlayerController _musicPlayerController;
        private readonly MusicSelectionPageModel _pageModel;

        public MusicSelectedAmplitudeEventSignalEmitter(SignalBus signalBus, MusicPlayerController musicPlayerController, MusicSelectionPageModel pageModel) : base(signalBus)
        {
            _musicPlayerController = musicPlayerController;
            _pageModel = pageModel;
        }

        public override void Initialize()
        {
            _musicPlayerController.MusicDownloaded += OnMusicDownloaded;
        }

        public override void Dispose()
        {
            _musicPlayerController.MusicDownloaded -= OnMusicDownloaded;
        }

        private void OnMusicDownloaded(IPlayableMusic music, float loadingTime)
        {
            if (!_pageModel.IsOpened) return;
            
            Emit(new MusicSelectedAmplitudeEvent(music, loadingTime));
        }
    }
}