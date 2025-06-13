using Modules.Amplitude;

namespace UIManaging.Pages.PostRecordEditor.AssetButtons
{
    internal sealed class OutfitAssetButton : BaseAssetButton
    {
        protected override void OnClicked()
        {
            EditorPageModel.OnOutfitButtonPressed();
            AmplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.OUTFIT_BUTTON_CLICKED);
        }
    }
}
