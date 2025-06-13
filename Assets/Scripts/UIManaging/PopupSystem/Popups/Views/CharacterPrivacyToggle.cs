using Bridge.Models.ClientServer.Assets;
using Utils;

namespace UIManaging.PopupSystem.Popups.Views
{
    internal sealed class CharacterPrivacyToggle: PrivacyToggleBase<CharacterAccess>
    {
        public override void SetValue(CharacterAccess access)
        {
            targetToggle.isOn = targetAccess == access;
        }

        protected override string ToText() => targetAccess.ToText();
    }
}