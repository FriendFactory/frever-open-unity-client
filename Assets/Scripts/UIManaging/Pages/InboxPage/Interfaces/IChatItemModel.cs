using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer;

namespace UIManaging.Pages.InboxPage.Interfaces
{
    public interface IChatItemModel
    {
        event Action ItemUpdated;
        event Action<long> OpenChatRequested;

        long Id { get; }
        string ChatName { get; }
        string LastMessage { get; }
        int UnreadMessagesCounter { get; }
        DateTime LastMessageTime { get; }
        IReadOnlyList<GroupShortInfo> Users { get; }
        IReadOnlyList<long> BlockedUserIds { get; }
        bool IsMuted { get; }

        void OpenChat();
    }
}