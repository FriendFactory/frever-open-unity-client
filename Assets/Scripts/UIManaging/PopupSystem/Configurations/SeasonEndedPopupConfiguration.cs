using System;

namespace UIManaging.PopupSystem.Configurations
{
    public class SeasonEndedPopupConfiguration : InformationPopupConfiguration
    {
        public Action OnButtonClick;
        public string ButtonText;

        public SeasonEndedPopupConfiguration()
        {
            PopupType = PopupType.SeasonEndedPopup;
        }
    }
}