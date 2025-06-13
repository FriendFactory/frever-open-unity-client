using UIManaging.Common.Toggles;

namespace UIManaging.Pages.AppSettingsPage.UI
{
    internal sealed class AppSettingsPageToggle : SettingsToggle
    {
        protected override void OnValueChanged(bool value)
        {
            OnTransitionStarted();
            PlayToggleState(value);
            base.OnValueChanged(value);
        }
    }
}
