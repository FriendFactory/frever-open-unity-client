using Modules.Amplitude;

namespace UIManaging.Pages.PostRecordEditor.AssetButtons
{
    internal sealed class CameraAnimationAssetButton : BaseAssetButton
    {
        protected override void OnClicked()
        {
            EditorPageModel.OnCameraButtonClicked();
            AmplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.CAMERA_BUTTON_CLICKED);
        }
    }
}
