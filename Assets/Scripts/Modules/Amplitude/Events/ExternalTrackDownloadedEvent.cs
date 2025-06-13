using Modules.Amplitude.Events.Core;

namespace Modules.Amplitude.Events
{
    public class ExternalTrackDownloadedEvent : BaseAmplitudeEvent
    {
        public override string Name => "external_track_downloaded";

        public ExternalTrackDownloadedEvent(long trackId)
        {
            _eventProperties.Add("Track ID", trackId);
        }
    }
}