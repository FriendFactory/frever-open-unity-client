using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Abstract;
using Bridge;
using Bridge.Models.ClientServer.Chat;
using Bridge.Services.UserProfile;
using UIManaging.Pages.Common.UsersManagement;

namespace UIManaging.PopupSystem.Popups.Share
{
    internal sealed class ShareSelectionDataManager : IAsyncInitializable 
    {
        private const int PAGE_SIZE = 5;
        
        private readonly ShareSelectionChatsNextPageButtonModel _chatsNextPageButtonModel;
        private readonly ShareSelectionFriendsNextPageButtonModel _friendsNextPageButtonModel;
        private readonly ShareSelectionListModel _shareSelectionListModel;

        private readonly FriendsPaginatedDataSource _friendsDataSource;
        private readonly ChatsPaginatedDataSource _chatsDataSource;

        private bool _isLocked;
        
        public ShareSelectionDataManager(IBridge bridge, LocalUserDataHolder userDataHolder, ShareSelectionChatsNextPageButtonModel chatsNextPageButtonModel,
            ShareSelectionFriendsNextPageButtonModel friendsNextPageButtonModel, ShareSelectionPanelModel shareSelectionPanelModel, 
            SelectionModelItemFactory factory, bool isLocked)
        {
            _chatsNextPageButtonModel = chatsNextPageButtonModel;
            _friendsNextPageButtonModel = friendsNextPageButtonModel;
            _shareSelectionListModel = new ShareSelectionListModel(chatsNextPageButtonModel, friendsNextPageButtonModel, shareSelectionPanelModel, factory);

            _friendsDataSource = new FriendsPaginatedDataSource(bridge, PAGE_SIZE);
            _chatsDataSource = new ChatsPaginatedDataSource(bridge, userDataHolder, PAGE_SIZE);

            _isLocked = isLocked;
        }

        public bool IsInitialized { get; set; }
        public IShareSelectionListModel ShareSelectionListModel => _shareSelectionListModel;

        public event Action DataChanged;
        
        public async Task InitializeAsync(CancellationToken token)
        {
            var initializingTasks = new[] { _friendsDataSource.InitializeAsync(token), _chatsDataSource.InitializeAsync(token) };
            await Task.WhenAll(initializingTasks);
            
            if (token.IsCancellationRequested) return;
            
            if (!_friendsDataSource.IsInitialized || !_chatsDataSource.IsInitialized) return;

            IsInitialized = true;
        }

        public void CleanUp()
        {
            _friendsDataSource.CleanUp();
            _chatsDataSource.CleanUp();
            
            IsInitialized = false;
        }

        public void LoadFirstPage(string searchQuery)
        {
            var chats = _chatsDataSource.GetFirstPage(searchQuery);
            var friends = _friendsDataSource.GetFirstPage(searchQuery);
            
            UpdateChatsNextButtonState();
            UpdateFriendsNextButtonState();
            
            _shareSelectionListModel.AddItems(chats, friends, _isLocked);
        }

        public void LoadFriendsNextPage()
        {
            _friendsNextPageButtonModel.ChangeState(SearchNextButtonState.Busy);

            var nextPage = _friendsDataSource.GetNextPage();
            
            UpdateFriendsNextButtonState();

            _shareSelectionListModel.AddFriendItems(nextPage, _isLocked);
            
            DataChanged?.Invoke();
        }

        public void LoadChatsNextPage()
        {
            _chatsNextPageButtonModel.ChangeState(SearchNextButtonState.Busy);

            var nextPage = _chatsDataSource.GetNextPage();
            
            UpdateChatsNextButtonState();

            _shareSelectionListModel.AddChatItems(nextPage, _isLocked);

            DataChanged?.Invoke();
        }

        private void UpdateFriendsNextButtonState()
        {
            _friendsNextPageButtonModel.ChangeState(_friendsDataSource.IsLastPageLoaded ? SearchNextButtonState.Disabled: SearchNextButtonState.Enabled);
        }

        private void UpdateChatsNextButtonState()
        {
            _chatsNextPageButtonModel.ChangeState(_chatsDataSource.IsLastPageLoaded ? SearchNextButtonState.Disabled : SearchNextButtonState.Enabled);
        }
    }
}