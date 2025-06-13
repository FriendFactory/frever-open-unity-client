using Modules.Amplitude.Events.Core;
using Navigation.Args.Feed;
using Navigation.Core;

namespace Modules.Amplitude.Events.PageChange
{
    internal sealed class FeedPageChangeAmplitudeEventDecorator: AmplitudeEventDecorator
    {
        public FeedPageChangeAmplitudeEventDecorator(IDecoratableAmplitudeEvent wrappedEvent, PageArgs pageArgs) : base(wrappedEvent)
        {
            if (pageArgs is not BaseFeedArgs baseFeedArgs) return;

            _wrappedEvent.AddProperty(AmplitudeEventConstants.EventProperties.FEED_TYPE, baseFeedArgs.Name);
            _wrappedEvent.AddProperty(AmplitudeEventConstants.EventProperties.FEED_TAB_NAME, baseFeedArgs.VideoListType.ToString());
        }
    }
}