using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.ClientServer.Chat;
using Bridge.Models.ClientServer.Chat;
using Bridge.Services.UserProfile;
using Extensions;
using Navigation.Args;
using TMPro;
using UIManaging.Animated.Behaviours;
using UIManaging.Common.SearchPanel;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.UserSelection;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.PopupSystem.Popups.StartChat
{
    internal sealed class StartChatPopup: BasePopup<StartChatPopupConfiguration>
    {
        private const int MAX_SELECTED = 20;
        
        [SerializeField] private Button _outsideButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _saveButton;
        [SerializeField] private UserSelectionWidget _groupSearchWidget;
        [SerializeField] private SlideInOutBehaviour _slideInOut;

        [Inject] private IChatService _bridge;
        [Inject] private LocalUserDataHolder _dataHolder;

        private Action<long> _onSuccess;
        private UserSelectionPanelModel _userSelectionPanelModel;
        private bool _isCreatingChat;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void OnEnable()
        {
            _outsideButton.onClick.AddListener(OnOutsideButton);
            _backButton.onClick.AddListener(OnBackButton);
            _saveButton.onClick.AddListener(OnSaveButton);
        }

        private void OnDisable()
        {
            _outsideButton.onClick.RemoveListener(OnOutsideButton);
            _backButton.onClick.RemoveListener(OnBackButton);
            _saveButton.onClick.RemoveListener(OnSaveButton);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnConfigure(StartChatPopupConfiguration configuration)
        {
            _onSuccess = configuration.OnSuccess;
            
            _isCreatingChat = false;
            
            if (_userSelectionPanelModel != null)
            {
                _userSelectionPanelModel.ItemSelectionChanged -= OnItemSelectionChanged;
                _userSelectionPanelModel.Clear();
            }
            
            _userSelectionPanelModel = new UserSelectionPanelModel(MAX_SELECTED - 1, null, UserSelectionPageArgs.UsersFilter.Friends);
            _userSelectionPanelModel.ItemSelectionChanged += OnItemSelectionChanged;
        
            _groupSearchWidget.Initialize(_userSelectionPanelModel);
            
            _saveButton.interactable = _userSelectionPanelModel.SelectedItems.Count > 0;
            
            _slideInOut.SlideIn();
        }

        public override void Hide()
        {
            _slideInOut.SlideOut(base.Hide);
        }

        protected override void OnHidden()
        {
            _isCreatingChat = false;
            
            _groupSearchWidget.CleanUp();

            if (_userSelectionPanelModel != null)
            {
                _userSelectionPanelModel.ItemSelectionChanged -= OnItemSelectionChanged;
                _userSelectionPanelModel.Clear();
                _userSelectionPanelModel = null;
            }
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnOutsideButton()
        {
            if (_isCreatingChat)
            {
                return;
            }
            
            Hide();
        }
        
        private void OnBackButton()
        {
            if (_isCreatingChat)
            {
                return;
            }
            
            Hide();
        }
        
        private void OnSaveButton()
        {
            var userList = _userSelectionPanelModel.SelectedItems.Select(item => item.Id).ToList();
            userList.Add(_dataHolder.UserProfile.MainGroupId);

            CreateChat(userList);
        }

        private void OnItemSelectionChanged(UserSelectionItemModel item)
        {
            _saveButton.interactable = _userSelectionPanelModel.SelectedItems.Count > 0;
        }

        private async void CreateChat(List<long> ids)
        {
            if (_isCreatingChat)
            {
                return;
            }
            
            _isCreatingChat = true;

            var result = await _bridge.CreateChat(new SaveChatModel { GroupIds = ids });

            _isCreatingChat = false;

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
    }
}