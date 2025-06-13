using UIManaging.Common.Buttons;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel
{
    internal sealed class CaptionPositionHapticFeedback: EventBasedHapticFeedback<PositionAdjuster>
    {
        protected override void Subscribe()
        {
            Source.RulersEnabled += PlayFeedback;
        }

        protected override void Unsubscribe()
        {
            Source.RulersEnabled -= PlayFeedback;
        }
    }
}