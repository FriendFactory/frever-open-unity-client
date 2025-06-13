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
    internal sealed class ConfirmAddMembersPopup: BasePopup<ConfirmAddMembersPopupConfiguration>
    {
        [SerializeField] private Button _createBtn;
        [SerializeField] private Button _addBtn;
        [SerializeField] private Button _backBtn;
        [SerializeField] private Button _outsideBtn;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnConfigure(ConfirmAddMembersPopupConfiguration configuration)
        {
            _createBtn.onClick.AddListener(OnCreateButton);
            _addBtn.onClick.AddListener(OnAddButton);
            _backBtn.onClick.AddListener(Hide);
            _outsideBtn.onClick.AddListener(Hide);
        }

        protected override void OnHidden()
        {
            _createBtn.onClick.RemoveListener(OnCreateButton);
            _addBtn.onClick.RemoveListener(OnAddButton);
            _backBtn.onClick.RemoveListener(Hide);
            _outsideBtn.onClick.RemoveListener(Hide);
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnCreateButton()
        {
            Configs.OnCreate?.Invoke();
            Hide();
        }

        private void OnAddButton()
        {
            Configs.OnAdd?.Invoke();
            Hide();
        }
    }
}