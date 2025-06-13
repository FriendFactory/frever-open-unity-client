using Modules.Amplitude;

namespace UIManaging.Pages.LevelEditor.Ui.AssetButtons
{
    internal sealed class MemberAssetButton : BaseAssetButton
    {
        protected override void OnClicked()
        {
            LevelEditorPageModel.OnSwitchCharacterButtonClicked();
            AmplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.CHARACTER_BUTTON_CLICKED);
        }
    }
}
