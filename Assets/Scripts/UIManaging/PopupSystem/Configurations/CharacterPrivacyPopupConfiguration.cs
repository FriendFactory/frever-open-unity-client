using System;
using Bridge.Models.ClientServer.Assets;

namespace UIManaging.PopupSystem.Configurations
{
    public sealed class CharacterPrivacyPopupConfiguration: BasePrivacyPopupConfiguration<CharacterAccess>
    {
        public CharacterPrivacyPopupConfiguration(CharacterAccess access, Action<object> onClose = null)
            : base(access, PopupType.CharacterPrivacyPopup, onClose) { }
    }
}