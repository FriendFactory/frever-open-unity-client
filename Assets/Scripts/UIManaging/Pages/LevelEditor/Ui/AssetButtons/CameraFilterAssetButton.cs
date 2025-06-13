using Modules.Amplitude;

namespace UIManaging.Pages.LevelEditor.Ui.AssetButtons
{
    internal sealed class CameraFilterAssetButton : BaseAssetButton
    {
        protected override void OnClicked()
        {
            LevelEditorPageModel.OnFilterClicked();
            AmplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.CAMERA_FILTER_BUTTON_CLICKED);
        }
    }
}