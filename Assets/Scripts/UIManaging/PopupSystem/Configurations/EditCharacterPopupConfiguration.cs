using System;
using Bridge.Models.ClientServer.Assets;

namespace UIManaging.PopupSystem.Configurations
{
    public class EditCharacterPopupConfiguration : PopupConfiguration
    {
        public CharacterInfo Character;
        public Action<CharacterInfo> OnSelect;
        public Action<CharacterInfo> OnDelete;
        public Action<CharacterInfo> OnEdit;
    }
}