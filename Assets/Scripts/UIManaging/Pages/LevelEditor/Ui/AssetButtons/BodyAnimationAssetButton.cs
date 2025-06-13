using Modules.Amplitude;

namespace UIManaging.Pages.LevelEditor.Ui.AssetButtons
{
    internal sealed class BodyAnimationAssetButton : BaseAssetButton
    {
        protected override void OnClicked()
        {
            LevelEditorPageModel.OnBodyAnimationsButtonClicked();
            AmplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.BODY_ANIMATION_BUTTON_CLICKED);
        }
    }
}
