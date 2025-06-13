using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer;
using Bridge.Models.ClientServer.Chat;
using UIManaging.Pages.InboxPage.Interfaces;

namespace UIManaging.Pages.InboxPage.Models
{
    public class ChatItemModel: IChatItemModel
    {
        public event Action ItemUpdated;
        public event Action<long> OpenChatRequested;

        public long Id { get; private set; }
        public string ChatName { get; private set; }
        public string LastMessage { get; private set; }
        public int UnreadMessagesCounter { get; private set; }
        public DateTime LastMessageTime { get; private set; }
        public IReadOnlyList<GroupShortInfo> Users { get; private set; }
        public IReadOnlyList<long> BlockedUserIds { get; private set; }
        public bool IsMuted { get; private set; }
        

        public ChatItemModel(ChatShortInfo chatInfo, IReadOnlyList<long> blockedUserIds)
        {
            UpdateInfo(chatInfo, blockedUserIds);
        }

        public void UpdateInfo(ChatShortInfo chatInfo, IReadOnlyList<long> blockedUserIds)
        {
            Id = chatInfo.Id;
            ChatName = chatInfo.Title;
            LastMessage = chatInfo.LastMessageText;
            LastMessageTime = chatInfo.LastMessageTime ?? DateTime.Now;
            UnreadMessagesCounter = chatInfo.NewMessagesCount;
            Users = chatInfo.Members;
            BlockedUserIds = blockedUserIds;
            IsMuted = chatInfo.MutedUntilTime.HasValue;
            
            ItemUpdated?.Invoke();
        }

        public void OpenChat()
        {
            OpenChatRequested?.Invoke(Id);
        }
    }
}