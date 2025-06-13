using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Chat;
using Bridge.Services.UserProfile;
using UIManaging.PopupSystem.Configurations;

namespace UIManaging.PopupSystem.Popups.Share
{
    internal sealed class SelectionModelItemFactory
    {
        private readonly ShareDestinationData _preselectedShareDestinations;

        public SelectionModelItemFactory(ShareDestinationData preselectedShareDestinations)
        {
            _preselectedShareDestinations = preselectedShareDestinations;
        }

        public ShareSelectionItemModel[] CreateChatModels(IEnumerable<ChatShortInfo> chatInfos, bool isLocked)
        {
            return chatInfos.Select(chatInfo => CreateChatModel(chatInfo, isLocked)).ToArray();
        }
        
        public ShareSelectionItemModel CreateChatModel(ChatShortInfo chatInfo, bool isLocked)
        {
            var membersCount = chatInfo.Members?.Length ?? 0;
                
            var isPreselected = _preselectedShareDestinations?.Chats != null &&
                                _preselectedShareDestinations.Chats.Any(x => x.Id == chatInfo.Id);
                
            if (chatInfo.Type == ChatType.Crew)
            {
                return new ShareSelectionCrewChatsItemModel(chatInfo, isPreselected) { IsLocked = isLocked };
            }
                
            return membersCount > 2
                ? new ShareSelectionGroupChatsItemModel(chatInfo, isPreselected) { IsLocked = isLocked }
                : new ShareSelectionDirectChatsItemModel(chatInfo, isPreselected) { IsLocked = isLocked };
        }
        
        public List<ShareSelectionItemModel> CreateFriendSelectionModels(IEnumerable<Profile> friends, bool isLocked)
        {
            return friends.Select(friend => new ShareSelectionFriendsItemModel(friend, IsSelected(friend)) 
                                      { IsLocked = isLocked || friend.ChatAvailableAfterTime > DateTime.UtcNow  } as ShareSelectionItemModel).ToList();
        }
        
        private bool IsSelected(Profile profile)
        {
            if (_preselectedShareDestinations?.Users == null)
            {
                return false;
            }
            var preselectedUsers = _preselectedShareDestinations.Users;
            return preselectedUsers.Any(x => x.MainGroupId == profile.MainGroupId);
        }
    }
}