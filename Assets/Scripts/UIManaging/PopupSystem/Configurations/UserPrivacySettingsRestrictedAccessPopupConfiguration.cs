using System;

namespace UIManaging.PopupSystem.Configurations
{
    public sealed class UserPrivacySettingsRestrictedAccessPopupConfiguration: PopupConfiguration
    {
        public readonly Action OnOkClicked;
        
        public UserPrivacySettingsRestrictedAccessPopupConfiguration(Action onOkClicked) : base(PopupType.ProfileAccessRestricted, null, string.Empty)
        {
            OnOkClicked = onOkClicked;
        }
    }
}