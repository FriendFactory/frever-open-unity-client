using System;

namespace UIManaging.PopupSystem.Configurations
{
    public class AlertPopupConfiguration : InformationPopupConfiguration
    {
        public Action OnConfirm;
        public string ConfirmButtonText;
    }
}
