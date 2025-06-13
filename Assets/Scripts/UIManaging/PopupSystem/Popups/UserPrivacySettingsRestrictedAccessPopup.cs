using System;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class UserPrivacySettingsRestrictedAccessPopup: BasePopup<UserPrivacySettingsRestrictedAccessPopupConfiguration>
    {
        [SerializeField] private Button _okButton;

        private Action _onOkClicked;
        
        protected override void OnConfigure(UserPrivacySettingsRestrictedAccessPopupConfiguration configuration)
        {
            _okButton.onClick.AddListener(()=>configuration.OnOkClicked?.Invoke());
        }
    }
}