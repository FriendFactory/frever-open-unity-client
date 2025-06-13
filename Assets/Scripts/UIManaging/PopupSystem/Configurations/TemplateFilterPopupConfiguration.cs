using System;

namespace UIManaging.PopupSystem.Configurations
{
    public class TemplateFilterPopupConfiguration : PopupConfiguration
    {
        public readonly int? CharactersCount;

        public TemplateFilterPopupConfiguration(int? charactersCount, Action<object> onClose) 
        {
            CharactersCount = charactersCount;
            OnClose = onClose;
            PopupType = PopupType.CharacterCountFilterPopup;
        }
    }
}