using Modules.Amplitude;

namespace UIManaging.Pages.PostRecordEditor.AssetButtons
{
    internal sealed class VfxAssetButton : BaseAssetButton
    {
        protected override void OnClicked()
        {
            EditorPageModel.OnVfxButtonClicked();
            AmplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.VFX_BUTTON_CLICKED);
        }
    }
}
