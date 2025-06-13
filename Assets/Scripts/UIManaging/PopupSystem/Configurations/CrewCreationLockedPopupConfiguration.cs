using System;

namespace UIManaging.PopupSystem.Configurations
{
    public class CrewCreationLockedPopupConfiguration : PopupConfiguration
    {
        public CrewCreationLockedPopupConfiguration(Action<object> onClose = null) 
            : base(PopupType.CrewCreationLocked, onClose, string.Empty)
        {

        }
    }
}