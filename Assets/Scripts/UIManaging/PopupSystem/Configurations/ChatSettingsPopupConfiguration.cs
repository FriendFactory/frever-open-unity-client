using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer;
using Bridge.Models.ClientServer.Chat;

namespace UIManaging.PopupSystem.Configurations
{
    public sealed class ChatSettingsPopupConfiguration : PopupConfiguration
    {
        public readonly ChatInfo ChatInfo;
        public readonly int MaxMembers;
        public readonly Action<bool> OnMuted;
        
        public ChatSettingsPopupConfiguration(ChatInfo chatInfo, int maxMembers, Action<bool> onMuted, Action<object> onClose = null) 
            : base(PopupType.ChatSettings, onClose)
        {
            ChatInfo = chatInfo;
            MaxMembers = maxMembers;
            OnMuted = onMuted;
        }
    }
}