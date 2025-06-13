using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge.ClientServer.Chat;
using Bridge.Models.ClientServer;
using Bridge.Models.ClientServer.Chat;
using UnityEngine;

namespace UIManaging.Pages.Crews
{
    public class MessagesListLoader : IChatModel
    {
        private const int PAGE_SIZE = 20;

        private readonly IChatService _bridge;
        private readonly List<ChatMessage> _models = new List<ChatMessage>();

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public long ChatId { get; }
        public string ChatName { get; }
        public ChatType ChatType { get; }
        public IReadOnlyList<ChatMessage> Messages => _models;
        public IReadOnlyList<GroupShortInfo> Members { get; }
        public bool IsAwaitingData { get; private set; }

        private object LastLoadedItemId => _models?.Count > 0 ? _models[_models.Count - 1].Id : (long?) null;
        private object FirstLoadedItemId => _models?.Count > 0 ? _models[0].Id : (long?) null;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<ChatMessage[]> NewPageAppended;
        public event Action<ChatMessage[]> NewPagePrepended;

        public event Action StartPageLoaded;
        public event Action EndPageLoaded;

        public event Action<ChatMessage> ReplyRequested;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public MessagesListLoader(IChatService bridge, long chatId, string chatName, ChatType chatType, IEnumerable<GroupShortInfo> members)
        {
            _bridge = bridge;
            ChatId = chatId;
            ChatName = chatName;
            ChatType = chatType;
            Members = new List<GroupShortInfo>(members);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async void DownloadFirstPage()
        {
            if (_models.Count > 0)
            {
                await DownloadNextPage();
            }
            else
            {
                await DownloadPrevPage();
            }
        }

        public async Task<bool> DownloadPrevPage()
        {
            IsAwaitingData = true;

            var pageSize = _models.Count > 0 ? PAGE_SIZE + 1 : PAGE_SIZE;
            var models = await DownloadModelsInternal(LastLoadedItemId, pageSize);

            if (models == null)
            {
                IsAwaitingData = false;
                return false;
            }

            models = _models.Count > 0 ? models.Skip(1).ToArray() : models;
            if (models.Length == 0)
            {
                StartPageLoaded?.Invoke();
                IsAwaitingData = false;
                return false;
            }

            _models.AddRange(models);
            NewPagePrepended?.Invoke(models);

            IsAwaitingData = false;
            return true;
        }

        public async Task<bool> DownloadNextPage()
        {
            IsAwaitingData = true;

            var models = await DownloadModelsInternal(FirstLoadedItemId, 0, PAGE_SIZE);

            if (models == null)
            {
                IsAwaitingData = false;
                return false;
            }

            if (models.Length == 0)
            {
                EndPageLoaded?.Invoke();
                IsAwaitingData = false;
                return false;
            }

            _models.InsertRange(0, models);
            NewPageAppended?.Invoke(models);

            IsAwaitingData = false;
            return true;
        }

        public void Reply(ChatMessage message)
        {
            ReplyRequested?.Invoke(message);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private async Task<ChatMessage[]> DownloadModelsInternal(object targetId, int takeNext, int takePrevious = 0,
            CancellationToken token = default)
        {
            var result = await _bridge.GetChatMessages(ChatId, (long?) targetId, takeNext, takePrevious, token);

            if (result.IsError)
            {
                Debug.LogError($"Unable to load chat messages. Reason: {result.ErrorMessage}");
            }

            return result.Models;

        }
    }
}