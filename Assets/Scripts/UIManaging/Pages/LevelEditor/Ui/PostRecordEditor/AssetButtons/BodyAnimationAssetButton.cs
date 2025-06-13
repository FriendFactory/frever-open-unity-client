using Modules.Amplitude;

namespace UIManaging.Pages.PostRecordEditor.AssetButtons
{
    internal sealed class BodyAnimationAssetButton : BaseAssetButton
    {
        protected override void OnClicked()
        {
            EditorPageModel.OnBodyAnimationsButtonClicked();
            AmplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.BODY_ANIMATION_BUTTON_CLICKED);
        }
    }
}
