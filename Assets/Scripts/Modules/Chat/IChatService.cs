using System;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Chat;
using Bridge.Results;
using JetBrains.Annotations;

namespace Modules.Chat
{
    /// <summary>
    /// Middle man service for having events on message has been posted
    /// </summary>
    public interface IChatService
    {
        event Action MessagePosted;
        
        Task<Result> PostMessage(long chatId, AddMessageModel model);
        Task<Result> PostMessage(SendMessageToGroupsModel model);
    }

    [UsedImplicitly]
    internal sealed class ChatService: IChatService
    {
        private readonly IBridge _bridge;
        
        public event Action MessagePosted;

        public ChatService(IBridge bridge)
        {
            _bridge = bridge;
        }

        public async Task<Result> PostMessage(long chatId, AddMessageModel model)
        {
            var result = await _bridge.PostMessage(chatId, model);
            if (result.IsSuccess)
            {
                MessagePosted?.Invoke();
            }
            return result;
        }

        public async Task<Result> PostMessage(SendMessageToGroupsModel model)
        {
            var result = await _bridge.PostMessage(model);
            if (result.IsSuccess)
            {
                MessagePosted?.Invoke();
            }
            return result;
        }
    }
}
