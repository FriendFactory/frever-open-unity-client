using System;
using Modules.Amplitude.Events.Core;
using Zenject;

namespace Modules.Amplitude.Signals
{
    public interface IAmplitudeEventSignalEmitter: IInitializable, IDisposable
    {
        void Emit(IAmplitudeEvent amplitudeEvent);
    }
}