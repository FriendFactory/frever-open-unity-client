using Modules.Amplitude;

namespace UIManaging.Pages.PostRecordEditor.AssetButtons
{
    internal sealed class MemberAssetButton : BaseAssetButton
    {
        protected override void OnClicked()
        {
            EditorPageModel.OnSwitchCharacterButtonClicked();
            AmplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.CHARACTER_BUTTON_CLICKED);
        }
    }
}
