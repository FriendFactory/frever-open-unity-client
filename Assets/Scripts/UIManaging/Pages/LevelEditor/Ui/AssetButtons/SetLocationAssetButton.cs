using Modules.Amplitude;

namespace UIManaging.Pages.LevelEditor.Ui.AssetButtons
{
    internal sealed class SetLocationAssetButton : BaseAssetButton
    {
        protected override void OnClicked()
        {
            LevelEditorPageModel.OnSetLocationsButtonClicked();
            AmplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.SET_LOCATION_BUTTON_CLICKED);
        }
    }
}
