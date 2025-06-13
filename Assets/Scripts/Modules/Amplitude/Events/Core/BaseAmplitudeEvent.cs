using System.Collections.Generic;

namespace Modules.Amplitude.Events.Core
{
    public abstract class BaseAmplitudeEvent: IAmplitudeEvent
    {
        protected readonly Dictionary<string, object> _eventProperties = new ();
        
        public abstract string Name { get; }
        public IReadOnlyDictionary<string, object> EventProperties => _eventProperties;
    }
}