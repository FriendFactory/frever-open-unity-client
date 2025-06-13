using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Chat;
using Bridge.Results;
using Extensions;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;
using UnityEngine;

namespace Modules.UserScenarios.Implementation.Common
{
    internal abstract class ChatPageExitStateBase<TContext> : ExitStateBase<TContext>
    {
        private readonly PageManager _pageManager;
        private readonly IBridge _bridge;
        
        protected abstract ChatInfo ChatRequestedToOpenOnFinish { get; }
        protected abstract MessagePublishInfo MessagePublishInfo { get; }

        protected ChatPageExitStateBase(PageManager pageManager, IBridge bridge)
        {
            _pageManager = pageManager;
            _bridge = bridge;
        }

        public override async void Run()
        {
            var chatInfo = await GetTargetChat();
            var chatPageArgs = new ChatPageArgs(chatInfo, true);
            _pageManager.MoveNext(chatPageArgs);
        }
        
        protected Task<ChatInfo> GetTargetChat()
        {
            return ChatRequestedToOpenOnFinish != null ? Task.FromResult(ChatRequestedToOpenOnFinish) : GetChatFromSharingDestination(MessagePublishInfo);
        }
        
        private async Task<ChatInfo> GetChatFromSharingDestination(MessagePublishInfo messagePublishInfo)
        {
            if (messagePublishInfo == null || messagePublishInfo.ShareDestination.Chats.IsNullOrEmpty() && messagePublishInfo.ShareDestination.Users.IsNullOrEmpty())
            {
                Debug.LogError($"Failed to open chat, because context is missed");
                return null;
            }

            Result<ChatInfo> res;
            if (!messagePublishInfo.ShareDestination.Chats.IsNullOrEmpty())
            {
                var chatShortInfo = messagePublishInfo.ShareDestination.Chats.First();
                res = await _bridge.GetChatById(chatShortInfo.Id);
                if (!res.IsSuccess)
                {
                    Debug.LogError(res.ErrorMessage);
                }

                return res.Model;
            }

            var targetUserGroup = messagePublishInfo.ShareDestination.Users.First();
            res = await _bridge.GetDirectMessageChatByGroupId(targetUserGroup.MainGroupId);
            if (!res.IsSuccess)
            {
                Debug.LogError(res.ErrorMessage);
            }

            return res.Model;
        }
    }
}