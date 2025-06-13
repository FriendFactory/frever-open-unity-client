using Bridge.Models.ClientServer.Chat;

namespace UIManaging.Pages.Crews
{
    internal static class ChatExtensions
    {
        public static MessageType GetMessageType(this ChatMessage message)
        {
            return (MessageType) message.MessageTypeId;
        }

        public static bool IsMessageType(this ChatMessage message, MessageType messageType)
        {
            return message.MessageTypeId == (long) messageType;
        }
    }
}