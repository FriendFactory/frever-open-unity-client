using System;
using Abstract;
using Bridge;
using Bridge.Scripts.ClientServer.Chat;
using Common.Permissions;
using Extensions.DateTime;
using Modules.Amplitude;
using I2.Loc;
using Modules.Crew;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Crews.Sidebar
{
    internal class SidebarSettingsPanel : BaseContextDataView<SidebarSettingsPanelModel>
    {
        [SerializeField] private Button _notificationSettingsButton;
        [SerializeField] private TMP_Text _notificationMutedTimeText;
        [SerializeField] private Button _editButton;
        [SerializeField] private Button _transferButton;
        [SerializeField] private Button _deleteButton;
        [SerializeField] private Button _leaveButton;

        [Header("Localization")]
        [SerializeField] private LocalizedString _muteNotificationsTitle;
        [SerializeField] private LocalizedString _notificationMutedUntilText;
        [SerializeField] private LocalizedString _notificationMutedForeverText;
        
        [Inject] private AmplitudeManager _amplitudeManager;
        [Inject] private PageManager _pageManager;
        [Inject] private PopupManager _popupManager;
        [Inject] private LocalUserDataHolder _localUser;
        [Inject] private CrewService _crewService;
        [Inject] private IBridge _bridge;
        [Inject] private IPermissionsHelper _permissionsHelper;

        private void OnEnable()
        {
            _notificationSettingsButton.onClick.AddListener(OnNotificationSettingsButtonClick);
            _editButton.onClick.AddListener(OpenEditCrewOverlay);
            _transferButton.onClick.AddListener(OnTransferButtonClicked);
            _deleteButton.onClick.AddListener(OnDeleteButtonClicked);
            _leaveButton.onClick.AddListener(OnLeaveButtonClicked);
        }

        private void OnDisable()
        {
            _notificationSettingsButton.onClick.RemoveAllListeners();
            _editButton.onClick.RemoveAllListeners();
            _transferButton.onClick.RemoveAllListeners();
            _deleteButton.onClick.RemoveAllListeners();
            _leaveButton.onClick.RemoveAllListeners();
        }

        protected override void OnInitialized()
        {
            UpdateMuteUntilText();
        }

        private void OpenEditCrewOverlay()
        {
            var cfg = new EditCrewPopupConfiguration(ContextData.ThumbnailOwner, ContextData.CrewName, ContextData.CrewDescription,
                                                     ContextData.IsPublic, ContextData.LanguageId, null);

            _popupManager.SetupPopup(cfg);
            _popupManager.ShowPopup(cfg.PopupType, true);
        }

        private void OnTransferButtonClicked()
        {
            var cfg = new TransferOwnershipPopupConfiguration(ContextData.CrewId, ContextData.Members,
                                                              _localUser.GroupId);
            _popupManager.SetupPopup(cfg);
            _popupManager.ShowPopup(cfg.PopupType);
        }

        private void OnDeleteButtonClicked()
        {
            _crewService.DeleteCrew(OnSuccess);

            void OnSuccess()
            {
                _popupManager.CloseAllPopups();
                _pageManager.MoveNext(new HomePageArgs(), false);
            }
        }
        
        private void OnLeaveButtonClicked()
        {
            _crewService.TryLeaveCrew(OnSuccess);

            void OnSuccess()
            {
                _popupManager.CloseAllPopups();
                _pageManager.MoveNext(new HomePageArgs(), false);
            }
        }
        
        private async void OnNotificationSettingsButtonClick()
        {
            _notificationSettingsButton.enabled = false;
            
            var chat = await _bridge.GetChatById(_crewService.Model.ChatId);

            if (chat?.Model.MutedUntilTime.HasValue == true)
            {
                ApplyMuteSettings(MuteChatTimeOptions.None);
                _notificationSettingsButton.enabled = true;
                return;
            }
            
            _popupManager.SetupPopup(new MuteSettingsPopupConfiguration
            {
                PopupType = PopupType.MuteSettings,
                Title = _muteNotificationsTitle,
                OnClose = ApplyMuteSettings
            });
            
            _popupManager.ShowPopup(PopupType.MuteSettings, true);
            
            _notificationSettingsButton.enabled = true;
        }

        private async void ApplyMuteSettings(object option)
        {
            if (option == null) return;
            var muteOption = (MuteChatTimeOptions)option;

            if (muteOption == MuteChatTimeOptions.None
             && !_permissionsHelper.HasPermission(PermissionTarget.Notifications)) 
            {
                _permissionsHelper.RequestPermission(PermissionTarget.Notifications, null);
            }

            var result = await _bridge.MuteChatNotifications(ContextData.ChatId, muteOption);

            if (result.IsSuccess)
            {
                UpdateMuteUntilText();
            }
        }
        private async void UpdateMuteUntilText()
        {
            _notificationMutedTimeText.text = string.Empty;
            
            var chat = await _bridge.GetChatById(_crewService.Model.ChatId);
            
            if(chat?.Model.MutedUntilTime == null) return;

            if ((DateTime.MaxValue.ToUniversalTime() - chat.Model.MutedUntilTime).Value.TotalDays <= 0)
            {
                _notificationMutedTimeText.text =
                    $"{_notificationMutedForeverText}";
                return;
            }
            
            _notificationMutedTimeText.text =
                $"{_notificationMutedUntilText} {chat.Model.MutedUntilTime.Value.GetFormattedUntilDate()}";
        }
    }
}