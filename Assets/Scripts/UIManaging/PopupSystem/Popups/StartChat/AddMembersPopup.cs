using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.ClientServer.Chat;
using Bridge.Models.ClientServer.Chat;
using Navigation.Args;
using UIManaging.Animated.Behaviours;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.UserSelection;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.PopupSystem.Popups.StartChat
{
    internal sealed class AddMembersPopup: BasePopup<AddMembersPopupConfiguration>
    {
        private const int MAX_SELECTED = 20;
        
        [SerializeField] private Button _outsideButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _addButton;
        [SerializeField] private ChatUserSelectionWidget _groupSearchWidget;
        [SerializeField] private AddMembersSearchHandler _searchHandler;
        [SerializeField] private SlideInOutBehaviour _slideInOut;

        [Inject] private IChatService _bridge;
        [Inject] private LocalUserDataHolder _dataHolder;
        [Inject] private PopupManager _popupManager;

        private Action<long> _onSuccess;
        private UserSelectionPanelModel _userSelectionPanelModel;
        private bool _requestInProgress;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void OnEnable()
        {
            _outsideButton.onClick.AddListener(OnBackButton);
            _backButton.onClick.AddListener(OnBackButton);
            _addButton.onClick.AddListener(OnAddButton);
        }

        private void OnDisable()
        {
            _outsideButton.onClick.RemoveListener(OnBackButton);
            _backButton.onClick.RemoveListener(OnBackButton);
            _addButton.onClick.RemoveListener(OnAddButton);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnConfigure(AddMembersPopupConfiguration configuration)
        {
            _onSuccess = configuration.OnSuccess;
            
            _requestInProgress = false;
            
            _userSelectionPanelModel = new UserSelectionPanelModel(MAX_SELECTED - configuration.MemberIds.Count - 1, null, UserSelectionPageArgs.UsersFilter.Friends);
            _userSelectionPanelModel.ItemSelectionChanged += OnItemSelectionChanged;
            
            _groupSearchWidget.Initialize(_userSelectionPanelModel);
            _groupSearchWidget.UpdateDefaultUserCount(configuration.MemberIds.Count + 1);
            _searchHandler.ExcludedIds = Configs.MemberIds;
                
            _addButton.interactable = _userSelectionPanelModel.SelectedItems.Count > 0;
            
            _slideInOut.SlideIn();
        }

        public override void Hide()
        {
            _slideInOut.SlideOut(base.Hide);
        }

        protected override void OnHidden()
        {
            _requestInProgress = false;

            CleanUpGroupChatTab();
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnBackButton()
        {
            if (_requestInProgress)
            {
                return;
            }
            
            Hide();
        }
        
        private void OnAddButton()
        {
            var userList = _userSelectionPanelModel.SelectedItems.Select(item => item.Id).ToList();
            userList.Add(_dataHolder.UserProfile.MainGroupId);
            var config = new ConfirmAddMembersPopupConfiguration(() => CreateChat(userList), 
                                                                 () => AddMembers(userList));

            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
        }

        private void OnItemSelectionChanged(UserSelectionItemModel item)
        {
            _addButton.interactable = _userSelectionPanelModel.SelectedItems.Count > 0;
        }

        private void CleanUpGroupChatTab()
        {
            _groupSearchWidget.CleanUp();

            if (_userSelectionPanelModel != null)
            {
                _userSelectionPanelModel.ItemSelectionChanged -= OnItemSelectionChanged;
                _userSelectionPanelModel.Clear();
                _userSelectionPanelModel = null;
            }
        }

        private async void CreateChat(List<long> ids)
        {
            if (_requestInProgress)
            {
                return;
            }
            
            _requestInProgress = true;
            
            var result = await _bridge.CreateChat(new SaveChatModel { GroupIds = ids.Concat(Configs.MemberIds).ToList() });

            _requestInProgress = false;

            if (result.IsError)
            {
                Debug.LogError($"Failed to create chat, reason: {result.ErrorMessage}");
                return;
            }

            if (result.IsSuccess)
            {
                Hide();
                _onSuccess?.Invoke(result.CreatedChatId);
            }
        }
        
        private async void AddMembers(List<long> ids)
        {
            if (_requestInProgress)
            {
                return;
            }
            
            _requestInProgress = true;

            var result = await _bridge.InviteChatMembers(new InviteMembersModel { ChatId = Configs.ChatId, GroupIds = ids });

            _requestInProgress = false;

            if (result.IsError)
            {
                Debug.LogError($"Failed to create chat, reason: {result.ErrorMessage}");
                return;
            }

            if (result.IsSuccess)
            {
                Hide();
                _onSuccess?.Invoke(Configs.ChatId);
            }
        }
    }
}