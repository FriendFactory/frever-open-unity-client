using Bridge.Models.ClientServer.Chat;

namespace Extensions
{
    public static class ChatInfoExtensions
    {
        //temp mapping, which will be gone on 1.9 API, when we leave only 1 model for chats.
        //in 1.8 API we have 2 identical ChatInfo and ChatShortInfo, so we need to convert one into another.
        //We can't just replace them because we use protobuf deserialization, not JSON
        public static ChatShortInfo ToChatShortInfo(this ChatInfo chatInfo)
        {
            return new ChatShortInfo
            {
                Id = chatInfo.Id,
                LastMessageText = chatInfo.LastMessageText,
                LastMessageTime = chatInfo.LastMessageTime,
                Members = chatInfo.Members,
                NewMessagesCount = chatInfo.NewMessagesCount,
                Title = chatInfo.Title,
                Type = chatInfo.Type
            };
        }
    }
}