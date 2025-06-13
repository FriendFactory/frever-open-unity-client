using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bridge;
using Bridge.ClientServer.Chat;
using UIManaging.Pages.InboxPage.Interfaces;
using UnityEngine;

namespace UIManaging.Pages.InboxPage.Models
{
    public class ChatListModel: IChatListModel
    {
        private const int PAGE_SIZE = 8;
        
        public event Action ItemsChanged;
        public event Action<long> OpenChatRequested;
        
        public IReadOnlyList<IChatItemModel> Items => _items;

        private readonly List<IChatItemModel> _items = new List<IChatItemModel>();
        private readonly IBridge _bridge;

        private CancellationTokenSource _tokenSource;
        private bool _scrollEnd;
        private bool _requestInProgress;

        public ChatListModel(IBridge bridge)
        {
            _bridge = bridge;
            _tokenSource = new CancellationTokenSource();
        }

        public void CleanUp()
        {
            foreach (var item in _items)
            {
                item.OpenChatRequested -= OnOpenChat;
            }
            
            _items.Clear();

            if (_tokenSource == null)
            {
                return;
            }
            
            _tokenSource.Cancel();
            _tokenSource.Dispose();
            _tokenSource = null;

            _scrollEnd = false;
            _requestInProgress = false;
        }
        
        public async void RequestPage()
        {
            if (_scrollEnd || _requestInProgress)
            {
                return;
            }

            _requestInProgress = true;
            
            var chatsResult = await _bridge.GetMyChats(_items.Count, PAGE_SIZE + 1, _tokenSource.Token);

            if (chatsResult.IsError)
            {
                Debug.LogError($"Failed to retrieve chats, reason: {chatsResult.ErrorMessage}");
                _requestInProgress = false;
                return;
            }

            if (chatsResult.IsSuccess)
            {
                var blockedResult = await _bridge.GetBlockedProfiles();
                
                _requestInProgress = false;
                
                if (blockedResult.IsError)
                {
                    Debug.LogError($"Failed to retrieve blocked user list, reason: {blockedResult.ErrorMessage}");
                    return;
                }

                if (blockedResult.IsSuccess)
                {
                    var blockedIds = blockedResult.Profiles.Select(profile => profile.MainGroupId).ToList();
                    var amount = chatsResult.Models.Count();

                    if (amount > PAGE_SIZE)
                    {
                        amount = PAGE_SIZE;
                    }
                    else
                    {
                        _scrollEnd = true;
                    }
                    
                    foreach (var item in chatsResult.Models.Take(amount).Select(info => new ChatItemModel(info, blockedIds)))
                    {
                        item.OpenChatRequested += OnOpenChat;
                        _items.Add(item);
                    }
                }
                
            }
            
            ItemsChanged?.Invoke();
        }

        private void OnOpenChat(long chatId)
        {
            OpenChatRequested?.Invoke(chatId);
        }
    }
}