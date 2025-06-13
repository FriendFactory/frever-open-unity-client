using Modules.Amplitude;

namespace UIManaging.Pages.PostRecordEditor.AssetButtons
{
    internal sealed class SetLocationAssetButton : BaseAssetButton
    {
        protected override void OnClicked()
        {
            EditorPageModel.OnSetLocationsButtonClicked();
            AmplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.SET_LOCATION_BUTTON_CLICKED);
        }
    }
}
