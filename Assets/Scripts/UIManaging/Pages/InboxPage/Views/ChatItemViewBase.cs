using System.Globalization;
using System.Linq;
using Abstract;
using Bridge.Models.ClientServer;
using TMPro;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Pages.InboxPage.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.InboxPage.Views
{
    public abstract class ChatItemViewBase: BaseContextDataView<IChatItemModel>
    {
        private static readonly Color READ_MESSAGE_COLOR = new Color32(139, 141, 144, 255);
        private static readonly Color UNREAD_MESSAGE_COLOR = Color.white;
        
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _lastMessageText;
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private TextMeshProUGUI _unreadMessagesCounterText;
        [SerializeField] private GameObject _unreadMessagesCounter;
        [SerializeField] private Button _chatBtn;
        [SerializeField] private GameObject _mutedIcon;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        protected TextMeshProUGUI LastMessageText => _lastMessageText;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            ContextData.ItemUpdated += UpdateData;
            _chatBtn.onClick.AddListener(OnChatClick);
            UpdateData();
        }

        protected override void BeforeCleanup()
        {
            if (ContextData != null)
            {
                ContextData.ItemUpdated -= UpdateData;
            }
            
            _chatBtn.onClick.RemoveListener(OnChatClick);
            
            base.BeforeCleanup();
        }

        protected virtual void UpdateData()
        {
            _nameText.text = ContextData.ChatName;
            _lastMessageText.text = ContextData.LastMessage;
            _lastMessageText.color = ContextData.UnreadMessagesCounter > 0 ? UNREAD_MESSAGE_COLOR : READ_MESSAGE_COLOR;

            if (_timeText != null)
            {
                _timeText.text = ContextData.LastMessageTime.ToLocalTime().ToString("HH:mm", CultureInfo.InvariantCulture);;
            }
            
            _unreadMessagesCounter.SetActive(ContextData.UnreadMessagesCounter > 0);
            _unreadMessagesCounterText.text = ContextData.IsMuted ? string.Empty : ContextData.UnreadMessagesCounter.ToString();
            
            _mutedIcon.SetActive(ContextData.IsMuted);
        }

        protected void SetUserThumbnail(UserPortraitView view, GroupShortInfo user)
        {
            if (user?.MainCharacterId == null)
            {
                view.Initialize(new UserPortraitModel { UserState = UserPortraitModel.State.Missing });
                return;
            }
            
            var userPortraitModel = new UserPortraitModel
            {
                Resolution = Resolution._128x128,
                UserGroupId = user.Id,
                UserMainCharacterId = user.MainCharacterId ?? 0,
                MainCharacterThumbnail = user.MainCharacterFiles,
                UserState = ContextData.BlockedUserIds.Contains(user.Id) ? UserPortraitModel.State.Blocked : UserPortraitModel.State.Available
            };
            
            view.Initialize(userPortraitModel);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnChatClick()
        {
            ContextData.OpenChat();
        }
    }
}