using System;

namespace UIManaging.PopupSystem.Configurations
{
    public class DialogPopupConfiguration : InformationPopupConfiguration
    {
        public Action OnYes;
        public Action OnNo;
        public string YesButtonText;
        public string NoButtonText;
    }
}