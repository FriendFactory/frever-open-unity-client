using System;
using System.Linq;
using Abstract;
using Bridge;
using Bridge.Models.ClientServer.Chat;
using Bridge.Scripts.ClientServer.Chat;
using Common;
using Common.Permissions;
using Extensions;
using Extensions.DateTime;
using I2.Loc;
using Navigation.Core;
using TMPro;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.PopupSystem.Popups.ChatSettings
{
    internal sealed class ChatSettingsModel
    {
        public ChatInfo ChatInfo;
        public readonly int MaxMembers;
        public Action<bool> OnMuted;

        public bool IsGroup => ChatInfo.Type == ChatType.Group;
        
        public ChatSettingsModel(ChatInfo chatInfo, int maxMembers, Action<bool> onMuted)
        {
            ChatInfo = chatInfo;
            MaxMembers = maxMembers;
            OnMuted = onMuted;
        }
    }

    internal sealed class ChatSettingsPanel : BaseContextDataView<ChatSettingsModel>
    {
        private const int MANAGE_SECTION_MAX_HEIGHT = 825;

        [SerializeField] private RectTransform _contentParent;
        [SerializeField] private VerticalLayoutGroup _contentLayout;
        
        [Space] 
        [SerializeField] private RectTransform _membersParent;
        [SerializeField] private LayoutElement _membersScrollLayout;
        [SerializeField] private ChatMemberView _memberView;

        [Space] 
        [SerializeField] private Button _addButton;
        [SerializeField] private GameObject _addButtonDisabled;
        [SerializeField] private Button _muteButton;
        [SerializeField] private Button _renameButton;
        [SerializeField] private Button _reportButton;
        [SerializeField] private Button _deleteButton;

        [Inject] private PageManager _pageManager;
        [Inject] private PopupManager _popupManager;
        [Inject] private ChatMemberModel.Factory _factory;
        [Inject] private StartAwaiterHelper _startAwaiter;
        [Inject] private IBridge _bridge;
        
        [SerializeField] private TMP_Text _notificationMutedTimeText;
        
        [Header("Localization")]
        [SerializeField] private LocalizedString _muteNotificationsTitle;
        [SerializeField] private LocalizedString _notificationMutedForeverText;
        [SerializeField] private LocalizedString _notificationMutedUntilText;
        
        [Inject] private IPermissionsHelper _permissionsHelper;
        
        public event Action HideActionRequested;
        public event Action ReportActionRequested;
        public event Action DeleteActionRequested;
        public event Action<long> GoToChatActionRequested;

        private void OnEnable()
        {
            _addButton.onClick.AddListener(OnAddButtonClicked);
            _muteButton.onClick.AddListener(OnNotificationSettingsButton);
            _renameButton.onClick.AddListener(OnRenameButtonClicked);
            _reportButton.onClick.AddListener(OnReportButtonClicked);
            _deleteButton.onClick.AddListener(OnDeleteButtonClicked);

            _startAwaiter.AwaitStart(() =>
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_membersParent);
                var maxHeight = _contentParent.GetHeight() - MANAGE_SECTION_MAX_HEIGHT - _contentLayout.spacing;
                _membersScrollLayout.preferredHeight = Mathf.Clamp(_membersParent.rect.height, 0, maxHeight);
            }, transform);
        }

        private void OnDisable()
        {
            _addButton.onClick.RemoveAllListeners();
            _muteButton.onClick.RemoveAllListeners();
            _renameButton.onClick.RemoveAllListeners();
            _reportButton.onClick.RemoveAllListeners();
            _deleteButton.onClick.RemoveAllListeners();

            for (var i = _membersParent.childCount - 1; i >= 0; i--)
            {
                Destroy(_membersParent.GetChild(i).gameObject);
            }
        }

        protected override void OnInitialized()
        {
            foreach (var member in ContextData.ChatInfo.Members)
            {
                var view = Instantiate(_memberView, _membersParent, false);
                view.Initialize(_factory.Create(member, reload =>
                {
                    if (reload)
                    {
                        GoToChatActionRequested?.Invoke(ContextData.ChatInfo.Id);
                        return;
                    }

                    HideActionRequested?.Invoke();
                }));
            }

            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            var isFull = ContextData.ChatInfo.Members.Length >= ContextData.MaxMembers;

            _addButton.SetActive(!isFull && ContextData.IsGroup);
            _addButtonDisabled.SetActive(isFull && ContextData.IsGroup);
            _renameButton.SetActive(ContextData.IsGroup);
            
            UpdateMuteUntilText();
        }

        private void OnAddButtonClicked()
        {
            var config = new AddMembersPopupConfiguration(ContextData.ChatInfo.Id, 
                    ContextData.ChatInfo.Members.Select(member => member.Id).ToList(), chatId => GoToChatActionRequested?.Invoke(chatId));
            
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
        }

        private void OnNotificationSettingsButton()
        {
            if (ContextData.ChatInfo.MutedUntilTime.HasValue)
            {
                ApplyMuteSettings(MuteChatTimeOptions.None);
                return;
            }
                
            _popupManager.SetupPopup(new MuteSettingsPopupConfiguration
            {
                PopupType = PopupType.MuteSettings,
                Title = _muteNotificationsTitle,
                OnClose = ApplyMuteSettings
            });
            
            _popupManager.ShowPopup(PopupType.MuteSettings, true);
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

            var result = await _bridge.MuteChatNotifications(ContextData.ChatInfo.Id, muteOption);

            if (result.IsSuccess)
            {
                UpdateMuteUntilText();
            }
        }
        
        private async void UpdateMuteUntilText()
        {
            _notificationMutedTimeText.text = string.Empty;
        
            var chat = await _bridge.GetChatById(ContextData.ChatInfo.Id);
            
            ContextData.ChatInfo = chat.Model;
            ContextData.OnMuted?.Invoke(ContextData.ChatInfo.MutedUntilTime.HasValue);
            
            if(ContextData.ChatInfo.MutedUntilTime == null) return;

            if ((DateTime.MaxValue.ToUniversalTime() - ContextData.ChatInfo.MutedUntilTime).Value.TotalDays <= 0)
            {
                _notificationMutedTimeText.text =
                    $"{_notificationMutedForeverText}";
                return;
            }
            
            _notificationMutedTimeText.text =
                $"{_notificationMutedUntilText} {ContextData.ChatInfo.MutedUntilTime.Value.GetFormattedUntilDate()}";
        }

        private void OnRenameButtonClicked()
        {
            var config = new ChatChangeNamePopupConfiguration(ContextData.ChatInfo.Id, ContextData.ChatInfo.Members.Select(member => member.Id).ToList(),
                                                              obj =>
                                                              {
                                                                  if (obj is bool result && !result)
                                                                  {
                                                                      return;
                                                                  }

                                                                  GoToChatActionRequested?.Invoke(ContextData.ChatInfo.Id);
                                                              });
            
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
        }

        private void OnReportButtonClicked()
        {
            ReportActionRequested?.Invoke();
        }

        private void OnDeleteButtonClicked()
        {
            DeleteActionRequested?.Invoke();
        }
    }
}