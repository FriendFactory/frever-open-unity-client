using UIManaging.Common.Toggles;
using UnityEngine;

namespace UIManaging.Pages.PublishPage.Buttons
{
    [RequireComponent(typeof(TogglePlayerPrefsSync))]
    public class ContentAccessSettingsToggle: SettingsToggle
    {
        protected override void OnValueChanged(bool value)
        {
            base.OnValueChanged(value);
            
            PlayToggleState(value);
        }
    }
}