using Modules.Amplitude;

namespace UIManaging.Pages.Feed.Ui.Feed
{
    internal sealed class UseForRemixButton : RemixButton
    {
        protected override void OnClick()
        {
            AmplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.CLICK_USE_THIS_TEMPLATE);
            StartRemixSetup();
            InvokeButtonClicked();
        }
    }
}
