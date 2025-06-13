using Bridge.Models.ClientServer.Assets;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using Utils;

namespace UIManaging.PopupSystem.Popups.Views
{
    public sealed class CharacterPrivacyButton: BasePrivacyButton<CharacterAccess>
    {
        protected override string ToText(CharacterAccess access) => access.ToText();

        protected override BasePrivacyPopupConfiguration<CharacterAccess> GetPopupConfiguration()
        {
            var config = new CharacterPrivacyPopupConfiguration(Access, accessObj =>
            {
                if (accessObj is CharacterAccess access)
                {
                    Access = access;
                }
                else
                {
                    Debug.LogError("Incorrect character access format");
                }
            });

            return config;
        }
    }
}