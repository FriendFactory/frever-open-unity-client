using System.Collections.Generic;

namespace Modules.Amplitude.Events.Core
{
    public abstract class BaseDecoratableAmplitudeEvent: IDecoratableAmplitudeEvent
    {
        protected readonly Dictionary<string, object> _eventProperties = new ();
        
        public abstract string Name { get; }
        public IReadOnlyDictionary<string, object> EventProperties => _eventProperties;

        public void AddProperty(string key, object value)
        {
            _eventProperties[key] = value;
        }
    }
}