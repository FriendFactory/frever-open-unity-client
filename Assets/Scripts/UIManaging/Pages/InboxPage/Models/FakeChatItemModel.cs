using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer;
using UIManaging.Pages.InboxPage.Interfaces;

namespace UIManaging.Pages.InboxPage.Models
{
    public class FakeChatItemModel: IChatItemModel
    {
        public event Action ItemUpdated;
        public event Action<long> OpenChatRequested;

        public long Id { get; }
        public string ChatName { get; }
        public string LastMessage { get; }
        public int UnreadMessagesCounter { get; }
        public DateTime LastMessageTime { get; }
        public IReadOnlyList<GroupShortInfo> Users { get; }
        public IReadOnlyList<long> BlockedUserIds { get; }
        public bool IsMuted { get; }


        public FakeChatItemModel(long id, string lastMessage, int unreadMessagesCounter, GroupShortInfo user, bool isBlocked)
        {
            Id = id;
            LastMessage = lastMessage;
            UnreadMessagesCounter = unreadMessagesCounter;
            Users = new List<GroupShortInfo> { user };
            ChatName = user.Nickname;
            LastMessageTime = DateTime.Now;
            BlockedUserIds = isBlocked ? new List<long> { user.Id } : new List<long>();
        }

        public void Update()
        {
            ItemUpdated?.Invoke();
        }

        public void OpenChat()
        {
            OpenChatRequested?.Invoke(Id);
        }
    }
}