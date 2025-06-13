using System;
using JetBrains.Annotations;
using Modules.Amplitude.Signals;
using Newtonsoft.Json;
using Zenject;

namespace Modules.Amplitude
{
    [UsedImplicitly]
    public class AmplitudeEventSignalListener: IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;

        public AmplitudeEventSignalListener(SignalBus signalBus, AmplitudeManager amplitudeManager)
        {
            _signalBus = signalBus;
            _amplitudeManager = amplitudeManager;
        }

        private readonly AmplitudeManager _amplitudeManager;
        
        public void Initialize()
        {
            _signalBus.Subscribe<AmplitudeEventSignal>(OnSignalReceived);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<AmplitudeEventSignal>(OnSignalReceived);
        }

        private void OnSignalReceived(AmplitudeEventSignal signal)
        {
            var amplitudeEvent = signal.Event;
            var eventProperties = amplitudeEvent.EventProperties;
            
            if (eventProperties.Count > 0)
            {
                _amplitudeManager.LogEventWithEventProperties(amplitudeEvent.Name, eventProperties);
            }
            else
            {
                _amplitudeManager.LogEvent(amplitudeEvent.Name);
            }
        }
    }
}