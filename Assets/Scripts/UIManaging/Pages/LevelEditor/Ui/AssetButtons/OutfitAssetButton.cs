using Modules.Amplitude;

namespace UIManaging.Pages.LevelEditor.Ui.AssetButtons
{
    internal sealed class OutfitAssetButton : BaseAssetButton
    {
        protected override void OnClicked()
        {
            LevelEditorPageModel.OnOutfitButtonPressed();
            AmplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.OUTFIT_BUTTON_CLICKED);
        }
    }
}
