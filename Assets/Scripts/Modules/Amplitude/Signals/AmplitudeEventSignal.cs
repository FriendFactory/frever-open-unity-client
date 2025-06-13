using Modules.Amplitude.Events.Core;

namespace Modules.Amplitude.Signals
{
    public class AmplitudeEventSignal
    {
        public IAmplitudeEvent Event { get; }

        public AmplitudeEventSignal(IAmplitudeEvent @event)
        {
            Event = @event;
        }
    }
}