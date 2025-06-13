using Modules.Amplitude.Events.Core;

namespace Modules.Amplitude.Events.AppEvents
{
    internal sealed class AppQuitAmplitudeEvent: BaseAmplitudeEvent
    {
        public override string Name => AmplitudeEventConstants.EventNames.APPLICATION_QUIT;
    }
}