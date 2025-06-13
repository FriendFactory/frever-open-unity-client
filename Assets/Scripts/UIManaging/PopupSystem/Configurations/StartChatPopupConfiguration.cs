using System;

namespace UIManaging.PopupSystem.Configurations
{
    public class StartChatPopupConfiguration : PopupConfiguration
    {
        public Action<long> OnSuccess { get; }

        public StartChatPopupConfiguration(Action<long> onSuccess) : base(PopupType.StartChatPopup, null, "")
        {
            OnSuccess = onSuccess;
        }
    }
}
