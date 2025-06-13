using Modules.Amplitude;

namespace UIManaging.Pages.PostRecordEditor.AssetButtons
{
    internal sealed class CameraFilterAssetButton : BaseAssetButton
    {
        protected override void OnClicked()
        {
            EditorPageModel.OnFilterClicked();
            AmplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.CAMERA_FILTER_BUTTON_CLICKED);
        }
    }
}