using Modules.Amplitude.Events.Core;

namespace Modules.Amplitude.Events
{
    public class SearchForExternalTrackEvent: BaseAmplitudeEvent 
    {
        public override string Name => "external_track_search";

        public SearchForExternalTrackEvent(string searchQuery)
        {
            _eventProperties.Add("Search Query", searchQuery);
        }
    }
}