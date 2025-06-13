using Modules.Amplitude;
using Modules.Amplitude.Events.Core;

namespace UIManaging.Pages.UmaEditorPage.Ui.Amplitude
{
    internal sealed class WardrobeSelectedAmplitudeEvent: BaseAmplitudeEvent
    {
        public override string Name => AmplitudeEventConstants.EventNames.WARDROBE_SELECTED;
        
        public WardrobeSelectedAmplitudeEvent(long wardrobeId, float loadingTime)
        {
            _eventProperties.Add(AmplitudeEventConstants.EventProperties.ASSET_ID, wardrobeId);
            _eventProperties.Add(AmplitudeEventConstants.EventProperties.LOADING_TIME, loadingTime);
        }
    }
}