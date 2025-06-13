using Modules.Amplitude.Events.Core;
using Zenject;

namespace Modules.Amplitude.Signals
{
    public abstract class BaseAmplitudeEventSignalEmitter: IAmplitudeEventSignalEmitter
    {
        protected readonly SignalBus SignalBus;

        protected BaseAmplitudeEventSignalEmitter(SignalBus signalBus)
        {
            SignalBus = signalBus;
        }

        public abstract void Initialize();
        public abstract void Dispose();

        public void Emit(IAmplitudeEvent amplitudeEvent)
        {
            SignalBus.Fire(new AmplitudeEventSignal(amplitudeEvent));
        }

    }
}