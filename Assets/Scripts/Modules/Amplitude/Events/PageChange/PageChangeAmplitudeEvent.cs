using Extensions;
using Modules.Amplitude.Events.Core;
using Navigation.Core;

namespace Modules.Amplitude.Events.PageChange
{
    internal sealed class PageChangeAmplitudeEvent: BaseDecoratableAmplitudeEvent
    {
        public override string Name => AmplitudeEventConstants.EventNames.PAGE_CHANGE;

        public PageChangeAmplitudeEvent(PageId pageId, long elapsedTime)
        {
            _eventProperties.Add(AmplitudeEventConstants.EventProperties.PAGE_NAME, pageId.ToString());
            _eventProperties.Add(AmplitudeEventConstants.EventProperties.LOADING_TIME, elapsedTime.ToSecondsClamped());
        }
    }
}