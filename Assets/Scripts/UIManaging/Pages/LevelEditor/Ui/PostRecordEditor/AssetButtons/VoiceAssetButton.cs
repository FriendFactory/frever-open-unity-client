using Modules.Amplitude;

namespace UIManaging.Pages.PostRecordEditor.AssetButtons
{
    internal sealed class VoiceAssetButton : BaseAssetButton
    {
        protected override void OnClicked()
        {
            EditorPageModel.OnVoiceButtonClicked();
            AmplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.VOICE_FILTER_BUTTON_CLICKED);
        }
    }
}
