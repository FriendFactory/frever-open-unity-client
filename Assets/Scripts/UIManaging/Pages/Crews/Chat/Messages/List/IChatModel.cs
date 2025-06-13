using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bridge.Models.ClientServer;
using Bridge.Models.ClientServer.Chat;

namespace UIManaging.Pages.Crews
{
    public interface IChatModel
    {
        long ChatId { get; }
        string ChatName { get; }
        ChatType ChatType { get; }
        IReadOnlyList<ChatMessage> Messages { get; }
        IReadOnlyList<GroupShortInfo> Members { get; }
        bool IsAwaitingData { get; }

        event Action<ChatMessage[]> NewPageAppended;
        event Action<ChatMessage[]> NewPagePrepended;

        event Action StartPageLoaded;
        event Action EndPageLoaded;
        
        event Action<ChatMessage> ReplyRequested;

        void DownloadFirstPage();

        Task<bool> DownloadPrevPage();

        Task<bool> DownloadNextPage();
        
        void Reply(ChatMessage message);
    }
}