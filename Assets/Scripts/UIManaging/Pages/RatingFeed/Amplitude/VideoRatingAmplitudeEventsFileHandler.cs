using JetBrains.Annotations;
using Modules.Amplitude.Signals;
using Modules.TempSaves.Manager;
using Zenject;

namespace UIManaging.Pages.RatingFeed.Amplitude
{
    [UsedImplicitly]
    internal sealed class VideoRatingAmplitudeEventsFileHandler: UnsentDataFileHandler<VideoRatingCancelledAmplitudeEvent>, IInitializable
    {
        private const string FILE_NAME = "UnsentVideoRatingsAmplitudeEvent.txt";
        
        private readonly SignalBus _signalBus;
        
        public VideoRatingAmplitudeEventsFileHandler(TempFileManager tempFileManager, SignalBus signalBus) : base(tempFileManager, FILE_NAME)
        {
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            if (TryLoad(out var videoRatingCancelledAmplitudeEvent))
            {
                _signalBus.Fire(new AmplitudeEventSignal(videoRatingCancelledAmplitudeEvent));
            }
        }
    }
}