using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Chat;
using Bridge.Services.UserProfile;
using Extensions;
using UIManaging.Localization;
using Zenject;

namespace UIManaging.PopupSystem.Popups.Share
{
    internal sealed class ShareSelectionListModel : IShareSelectionListModel
    {
        [Inject] private ShareToPopupLocalization _localization;
        
        private readonly ShareSelectionChatsNextPageButtonModel _chatsNextPageButtonModel;
        private readonly ShareSelectionFriendsNextPageButtonModel _friendsNextPageButtonModel;
        private readonly ShareSelectionPanelModel _shareSelectionPanelModel;
        private readonly List<IShareSelectionItemModel> _items;
        private readonly SelectionModelItemFactory _selectionModelItemFactory;
        
        private int _chatItemsLastIndex;
        private int _friendsItemsLastIndex;

        public IReadOnlyList<IShareSelectionItemModel> Items => _items;

        public ShareSelectionListModel(ShareSelectionChatsNextPageButtonModel chatsNextPageButtonModel,
            ShareSelectionFriendsNextPageButtonModel friendsNextPageButtonModel,
            ShareSelectionPanelModel shareSelectionPanelModel, SelectionModelItemFactory selectionModelItemFactory)
        {
            ProjectContext.Instance.Container.Inject(this);
            
            _items = new List<IShareSelectionItemModel>(20);

            _chatsNextPageButtonModel = chatsNextPageButtonModel;
            _friendsNextPageButtonModel = friendsNextPageButtonModel;
            _shareSelectionPanelModel = shareSelectionPanelModel;
            _selectionModelItemFactory = selectionModelItemFactory;
        }

        public void AddItems(ChatShortInfo[] chats, Profile[] friends, bool isLocked)
        {
            _items.Clear();

            _chatItemsLastIndex = 0;
            _friendsItemsLastIndex = 0;
            
            _items.Add(new ShareSelectionCategoryModel(_localization.ChatsCategory));

            if (!chats.IsNullOrEmpty())
            {
                AddOrInsertChatItems(chats, isLocked);
            }
            else
            {
                _items.Add(new ShareSelectionEmptyResultsPanelModel());
            }

            _items.Add(_chatsNextPageButtonModel);

            _items.Add(new ShareSelectionCategoryModel(_localization.FriendsCategory));
            
            if (!friends.IsNullOrEmpty())
            {
                AddOrInsertFriendItems(friends, isLocked);
            }
            else
            {
                _items.Add(new ShareSelectionEmptyResultsPanelModel());
            }

            _items.Add(_friendsNextPageButtonModel);
        }

        public void AddChatItems(ChatShortInfo[] chats, bool isLocked)
        {
            AddOrInsertChatItems(chats, isLocked);
        }
        
        public void AddFriendItems(Profile[] friends, bool isLocked)
        {
            AddOrInsertFriendItems(friends, isLocked);
        }

        private void AddOrInsertChatItems(IEnumerable<ChatShortInfo> chats, bool isLocked)
        {
            var chatItems = chats
                           .Select(chat => _selectionModelItemFactory.CreateChatModel(chat, isLocked))
                           .ToList();
            
            OverrideItems(chatItems);
            _shareSelectionPanelModel.AddItems(chatItems);

            if (_chatItemsLastIndex == 0)
            {
                _items.AddRange(chatItems);
                
                _chatItemsLastIndex = _items.Count;
            }
            else
            {
                _items.InsertRange(_chatItemsLastIndex, chatItems);
                
                _chatItemsLastIndex += chatItems.Count;
                _friendsItemsLastIndex += chatItems.Count;
            }
        }

        private void AddOrInsertFriendItems(IEnumerable<Profile> friends, bool isLocked)
        {
            var friendItems = _selectionModelItemFactory.CreateFriendSelectionModels(friends, isLocked);
      
            OverrideItems(friendItems);
            _shareSelectionPanelModel.AddItems(friendItems);

            if (_friendsItemsLastIndex == 0)
            {
                _items.AddRange(friendItems);
                
                _friendsItemsLastIndex = _items.Count;
            }
            else
            {
                _items.InsertRange(_friendsItemsLastIndex, friendItems);
                
                _friendsItemsLastIndex += friendItems.Count;
            }
        }

        private void OverrideItems(List<ShareSelectionItemModel> items)
        {
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var alreadyExistedItem = _shareSelectionPanelModel.Items.FirstOrDefault(selectionItem => selectionItem.Id == item.Id);
                if (alreadyExistedItem != null)
                {
                    items[i] = alreadyExistedItem;
                }
            }
        }
    }
}