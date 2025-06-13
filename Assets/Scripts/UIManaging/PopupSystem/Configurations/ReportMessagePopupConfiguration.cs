using System;

namespace UIManaging.PopupSystem.Configurations
{
    public sealed class ReportMessagePopupConfiguration : PopupConfiguration
    {
        public readonly long ChatId;
        public readonly long MessageId;
        
        public ReportMessagePopupConfiguration(long chatId, long messageId, Action<object> onClose = null) 
            : base(PopupType.ReportMessage, onClose)
        {
            ChatId = chatId;
            MessageId = messageId;
        }
    }
}