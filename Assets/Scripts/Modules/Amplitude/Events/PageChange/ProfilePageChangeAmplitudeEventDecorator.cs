using Modules.Amplitude.Events.Core;
using Navigation.Args;
using Navigation.Core;

namespace Modules.Amplitude.Events.PageChange
{
    internal sealed class ProfilePageChangeAmplitudeEventDecorator: AmplitudeEventDecorator
    {
        public ProfilePageChangeAmplitudeEventDecorator(IDecoratableAmplitudeEvent wrappedEvent, PageArgs pageArgs) : base(wrappedEvent)
        {
            if (pageArgs is not UserProfileArgs profilePageArgs) return;
                
            _wrappedEvent.AddProperty(AmplitudeEventConstants.EventProperties.GROUP_ID, profilePageArgs.GroupId);
        }
    }
}