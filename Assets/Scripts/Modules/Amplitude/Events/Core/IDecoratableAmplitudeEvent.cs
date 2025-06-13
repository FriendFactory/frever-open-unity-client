namespace Modules.Amplitude.Events.Core
{
    public interface IDecoratableAmplitudeEvent: IAmplitudeEvent
    {
        public void AddProperty(string key, object value);
    }
}