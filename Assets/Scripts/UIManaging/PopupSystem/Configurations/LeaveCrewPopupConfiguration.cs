using System;

namespace UIManaging.PopupSystem.Configurations
{
    public class LeaveCrewPopupConfiguration : PopupConfiguration
    {
        public LeaveCrewPopupConfiguration(Action<object> onClose) : base(PopupType.LeaveCrew, onClose)
        {
                   
        }
    }
}