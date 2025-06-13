using System.Collections.Generic;

namespace Modules.Amplitude.Events.Core
{
    public abstract class AmplitudeEventDecorator : IDecoratableAmplitudeEvent 
    {
        protected readonly IDecoratableAmplitudeEvent _wrappedEvent;
        
        public string Name => _wrappedEvent.Name;
        public virtual IReadOnlyDictionary<string, object> EventProperties => _wrappedEvent.EventProperties;
        
        public void AddProperty(string key, object value)
        {
            _wrappedEvent.AddProperty(key, value);
        }

        protected AmplitudeEventDecorator(IDecoratableAmplitudeEvent wrappedEvent)
        {
            _wrappedEvent = wrappedEvent;
        }
    }
}