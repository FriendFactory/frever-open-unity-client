using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Chat;
using Extensions;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.Share
{
    internal sealed class ChatsPaginatedDataSource: PaginatedDataSource<ChatShortInfo>
    {
        private readonly LocalUserDataHolder _userDataHolder;

        protected override TimeSpan ExpirationCacheTimeSpan => TimeSpan.FromMinutes(1);

        public ChatsPaginatedDataSource(IBridge bridge, LocalUserDataHolder userDataHolder, int pageSize) : base(bridge, pageSize)
        {
            _userDataHolder = userDataHolder;
        }
        
        protected override List<ChatShortInfo> GetFilteredModels(string searchQuery)
        {
            return Models.Where(model => model.Title.Contains(searchQuery)).ToList();
        }

        protected override async Task<PageResult<ChatShortInfo>> GetModelsInternal(int take, int skip, CancellationToken token)
        {
            var chats = new List<ChatShortInfo>();
            
            var chatsResult = await Bridge.GetMyChats(skip, take, token);

            if (_userDataHolder.UserProfile.CrewProfile != null)
            {
                var crewChatResult = await Bridge.GetMyCrewChat(token);
                if (crewChatResult.IsSuccess)
                {
                    var chat = crewChatResult.Model;
                    chats.Add(chat);
                }
                else if (crewChatResult.IsRequestCanceled)
                {
                    return PageResult<ChatShortInfo>.Cancelled();
                }
                else
                {
                    Debug.LogError($"[{GetType().Name}] Failed to get my crew chat # {crewChatResult.ErrorMessage}");
                }
            }

            if (chatsResult.IsError)
            {
                if (!chats.IsNullOrEmpty())
                {
                    Debug.LogError($"[{GetType().Name}] Failed to get chats # {chatsResult.ErrorMessage}");
                }
                else
                {
                    return PageResult<ChatShortInfo>.Error(chatsResult.ErrorMessage);
                }
            }

            if (chatsResult.IsRequestCanceled && chats.IsNullOrEmpty()) return PageResult<ChatShortInfo>.Cancelled();
            
            chats.AddRange(chatsResult.Models);
            
            return PageResult<ChatShortInfo>.Success(chats.ToArray());
        }
    }
}