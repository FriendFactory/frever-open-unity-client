using Modules.Amplitude;

namespace UIManaging.Pages.LevelEditor.Ui.AssetButtons
{
    internal sealed class CameraAnimationAssetButton : BaseAssetButton
    {
        protected override void OnClicked()
        {
            LevelEditorPageModel.OnCameraButtonClicked();
            AmplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.CAMERA_BUTTON_CLICKED);
        }
    }
}
