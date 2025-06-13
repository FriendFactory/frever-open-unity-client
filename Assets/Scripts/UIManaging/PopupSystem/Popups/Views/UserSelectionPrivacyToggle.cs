using Bridge.Models.VideoServer;
using UnityEngine;
using Utils;

namespace UIManaging.PopupSystem.Popups.Views
{
    internal sealed class UserSelectionPrivacyToggle: PrivacyToggle
    {
        [SerializeField] private PrivacyPopup _privacyPopup;
        
        public override void SetValue(VideoAccess access)
        {
            targetToggle.isOn = targetAccess == access;
        }

        protected override string ToText() => _privacyPopup.SelectedProfilesCount > 0 
            ? $"{base.ToText()}: {_privacyPopup.SelectedProfilesCount}"
            : base.ToText();
    }
}