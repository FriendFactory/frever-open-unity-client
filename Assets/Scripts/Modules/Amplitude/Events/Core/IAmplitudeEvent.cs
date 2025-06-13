using System.Collections.Generic;

namespace Modules.Amplitude.Events.Core
{
    public interface IAmplitudeEvent
    {
        public string Name { get; }
        IReadOnlyDictionary<string, object> EventProperties { get; }
    }
}