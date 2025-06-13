using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bridge.ClientServer.Chat;
using Bridge.Models.ClientServer;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.InboxPage.Interfaces;
using UnityEngine;

namespace UIManaging.Pages.InboxPage.Models
{
    public class FakeChatListModel: IChatListModel
    {
        public event Action ItemsChanged;
        
        public IReadOnlyList<IChatItemModel> Items => _items;

        private readonly List<IChatItemModel> _items = new List<IChatItemModel>();
        private readonly LocalUserDataHolder _dataHolder;

        public FakeChatListModel(LocalUserDataHolder dataHolder)
        {
            _dataHolder = dataHolder;
        }
        
        public void RequestPage()
        {
            if (_items.Count == 0)
            {
                var profile = _dataHolder.UserProfile;
                var user = new GroupShortInfo
                {
                    Id = profile.MainGroupId,
                    Nickname = profile.NickName,
                    MainCharacterId = profile.MainCharacter.Id,
                    MainCharacterFiles = profile.MainCharacter.Files
                };
                
                _items.Add(new FakeChatItemModel(0, "This is last message", 0, user, false));
                _items.Add(new FakeChatItemModel(1, "This is also last message", 3, user, false));
                _items.Add(new FakeChatItemModel(2, "This is another last message", 0, user, false));
                _items.Add(new FakeChatItemModel(3, "This is repeating message", 0, user, false));
                _items.Add(new FakeChatItemModel(4, "This is repeating message", 0, user, false));
                _items.Add(new FakeChatItemModel(5, "This is repeating message", 0, user, false));
                _items.Add(new FakeChatItemModel(6, "This is repeating message", 0, user, false));
                _items.Add(new FakeChatItemModel(7, "This is repeating message", 0, user, false));
                _items.Add(new FakeChatItemModel(8, "This is repeating message", 0, user, false));
                _items.Add(new FakeChatItemModel(9, "This is repeating message", 0, user, false));
                
                ItemsChanged?.Invoke();
            }
        }
    }
}