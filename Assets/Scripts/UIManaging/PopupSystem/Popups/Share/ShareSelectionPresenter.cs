using System;
using System.Collections.Generic;
using System.Threading;
using Bridge;
using Common.Abstract;
using Extensions;
using UIManaging.Common.SearchPanel;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using Zenject;

namespace UIManaging.PopupSystem.Popups.Share
{
    public sealed class ShareSelectionContext
    {
        public string CompleteButtonText;
        public ShareDestinationData Preselected;
        public Action<ShareDestinationData> OnConfirmed;
        public bool BlockConfirmButtonIfNoSelection;
    }

    public sealed class ShareSelectionPresenter : BaseContextPanel<ShareSelectionContext>
    {
        [SerializeField] private SearchPanelView _searchPanel;
        [SerializeField] private ConfirmButton _confirmButton;
        [SerializeField] private GameObject _loadingIcon;
        [Header("Selection Panel")]
        [SerializeField] private ShareSelectionList _shareSelectionList;
        [SerializeField] private ShareSelectionPanel _shareSelectionPanel;
        [SerializeField] private ShareSelectionPanelAnimator _selectionPanelAnimator;

        [Inject] private IBridge _bridge;
        [Inject] private LocalUserDataHolder _localUserDataHolder;
        
        private ShareSelectionChatsNextPageButtonModel _chatsNextPageButtonModel;
        private ShareSelectionFriendsNextPageButtonModel _friendsNextPageButtonModel;
        private ShareSelectionDataManager _shareSelectionDataManager;
        private ShareSelectionPanelModel _shareSelectionPanelModel;
        private CancellationTokenSource _cancellationTokenSource;

        private string SearchQuery { get; set; }
        
        protected override async void OnInitialized()
        {
            _chatsNextPageButtonModel = new ShareSelectionChatsNextPageButtonModel("Chats");
            _friendsNextPageButtonModel = new ShareSelectionFriendsNextPageButtonModel("Friends");
            var factory = new SelectionModelItemFactory(ContextData.Preselected);
            var isLocked = _localUserDataHolder.UserProfile.ChatAvailableAfterTime > DateTime.UtcNow;
            
            List<ShareSelectionItemModel> preselectedItemViewModel = null;
            if (ContextData.Preselected != null && ContextData.Preselected.HasAny)
            {
                preselectedItemViewModel = new List<ShareSelectionItemModel>();
                if (!ContextData.Preselected.Chats.IsNullOrEmpty())
                {
                    preselectedItemViewModel.AddRange(factory.CreateChatModels(ContextData.Preselected.Chats, isLocked));
                }

                if (!ContextData.Preselected.Users.IsNullOrEmpty())
                {
                    preselectedItemViewModel.AddRange(factory.CreateFriendSelectionModels(ContextData.Preselected.Users, isLocked));
                }
            }
            _shareSelectionPanelModel = new ShareSelectionPanelModel(100, preselectedItemViewModel,null, ContextData.CompleteButtonText, ContextData.OnConfirmed, ContextData.BlockConfirmButtonIfNoSelection);
            _shareSelectionDataManager = new ShareSelectionDataManager(_bridge, _localUserDataHolder,
                                                                       _chatsNextPageButtonModel,
                                                                       _friendsNextPageButtonModel,
                                                                       _shareSelectionPanelModel,
                                                                       factory,
                                                                        isLocked);
            
            _cancellationTokenSource = new CancellationTokenSource();
            _confirmButton.Interactable = false;
            _confirmButton.Text = ContextData.CompleteButtonText;
            try
            {
                _loadingIcon.SetActive(true);
                await _shareSelectionDataManager.InitializeAsync(_cancellationTokenSource.Token);
                _loadingIcon.SetActive(false);
                if (!_shareSelectionDataManager.IsInitialized) return;

                InitializeComponents();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        protected override void BeforeCleanUp()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
            }

            SearchQuery = string.Empty;
            
            ClearComponents();
        }

        private void InitializeComponents()
        {
            _chatsNextPageButtonModel.Clicked += OnChatsNextPageRequested;
            _friendsNextPageButtonModel.Clicked += OnFriendsNextPageRequested;

            _shareSelectionDataManager.DataChanged += OnModelChanged;

            _searchPanel.InputCleared += OnInputCleared;
            _searchPanel.InputCompleted += OnSearchCompleted;

            _shareSelectionDataManager.LoadFirstPage(SearchQuery);
            _shareSelectionList.Initialize(_shareSelectionDataManager.ShareSelectionListModel);
            _shareSelectionPanel.Initialize(_shareSelectionPanelModel);
            _selectionPanelAnimator.Initialize(_shareSelectionPanelModel);
            _confirmButton.Initialize(_shareSelectionPanelModel);
        }

        private void ClearComponents()
        {
            if (!_shareSelectionDataManager.IsInitialized) return;
            
            _chatsNextPageButtonModel.Clicked -= OnChatsNextPageRequested;
            _friendsNextPageButtonModel.Clicked -= OnFriendsNextPageRequested;

            _shareSelectionDataManager.DataChanged -= OnModelChanged;

            _searchPanel.InputCleared -= OnInputCleared;
            _searchPanel.InputCompleted -= OnSearchCompleted;
            
            _shareSelectionDataManager.CleanUp();
            _shareSelectionPanel.CleanUp();
            _shareSelectionList.CleanUp();
            _selectionPanelAnimator.CleanUp();
            _confirmButton.CleanUp();
            _shareSelectionPanelModel.Clear();
            _searchPanel.Clear();
        }

        private void OnModelChanged()
        {
            _shareSelectionList.ReloadData();
        }

        private void OnFriendsNextPageRequested()
        {
            _shareSelectionDataManager.LoadFriendsNextPage();
        }

        private void OnChatsNextPageRequested()
        {
            _shareSelectionDataManager.LoadChatsNextPage();
        }

        private void OnInputCleared()
        {
            SearchQuery = string.Empty;
            
            ReloadData();
        }

        private void OnSearchCompleted(string searchQuery)
        {
            SearchQuery = searchQuery;
            
            ReloadData();
        }

        private void ReloadData()
        {
            _shareSelectionPanelModel.ClearNonSelected();
            
            _shareSelectionDataManager.LoadFirstPage(SearchQuery);
            
            _shareSelectionPanel.ReloadData();
            _shareSelectionList.ReloadData();
        }
    }
}