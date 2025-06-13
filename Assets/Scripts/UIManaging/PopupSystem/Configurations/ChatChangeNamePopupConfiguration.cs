using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer;
using Bridge.Models.ClientServer.Chat;

namespace UIManaging.PopupSystem.Configurations
{
    public sealed class ChatChangeNamePopupConfiguration : PopupConfiguration
    {
        public long ChatId { get; }
        public List<long> Members { get; }

        public ChatChangeNamePopupConfiguration(long chatId, List<long> members, Action<object> onClose = null) 
            : base(PopupType.ChatChangeName, onClose)
        {
            ChatId = chatId;
            Members = members;
        }
    }
}