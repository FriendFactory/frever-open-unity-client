using Modules.Amplitude;

namespace UIManaging.Pages.LevelEditor.Ui.AssetButtons
{
    internal sealed class VfxAssetButton : BaseAssetButton
    {
        protected override void OnClicked()
        {
            LevelEditorPageModel.OnVfxButtonClicked();
            AmplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.VFX_BUTTON_CLICKED);
        }
    }
}
