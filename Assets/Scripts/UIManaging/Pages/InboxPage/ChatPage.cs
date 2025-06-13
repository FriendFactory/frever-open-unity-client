using System;
using System.Threading;
using Bridge.ClientServer.Chat;
using Extensions;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Pages.Common.Panels;
using UIManaging.Pages.Crews;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.InboxPage
{
    internal sealed class ChatPage: GenericPage<ChatPageArgs>
    {
        private const int MAX_MEMBERS = 20;
        
        [SerializeField] private ChatView _chatView;
        [SerializeField] private TextMeshProUGUI _chatName;
        [SerializeField] private GameObject _mutedIcon;
        [SerializeField] private Button _backBtn;
        [SerializeField] private Button _optionsBtn;
        [SerializeField] private Button _createButton; 
        [SerializeField] private PostTypeSelectionPanel _postTypeSelectionPanel;
        [SerializeField] private Button _closePostTypePanelButton;
        
        [Inject] private IChatService _bridge;
        [Inject] private PopupManager _popupManager;

        private MessagesListLoader _chatModel;
        private CancellationTokenSource _tokenSource;
        
        public override PageId Id => PageId.ChatPage;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            _backBtn.onClick.AddListener(OnBackButton);
            _optionsBtn.onClick.AddListener(OnOptionsButton);
            _createButton.onClick.AddListener(OnCreateButtonClicked);
            _closePostTypePanelButton.onClick.AddListener(ClosePostTypeSelectionPanel);
            _tokenSource = new CancellationTokenSource();
        }
        
        private void OnDisable()
        {
            _tokenSource.CancelAndDispose();
            
            _backBtn.onClick.RemoveListener(OnBackButton);
            _optionsBtn.onClick.RemoveListener(OnOptionsButton);
            _createButton.onClick.RemoveListener(OnCreateButtonClicked);
            _closePostTypePanelButton.onClick.RemoveListener(ClosePostTypeSelectionPanel);
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInit(PageManager pageManager)
        {
        }

        protected override void OnDisplayStart(ChatPageArgs args)
        {
            base.OnDisplayStart(args);

            _chatName.text = OpenPageArgs.ChatInfo.Title;
            _chatModel = new MessagesListLoader(_bridge, OpenPageArgs.ChatInfo.Id, OpenPageArgs.ChatInfo.Title, OpenPageArgs.ChatInfo.Type, OpenPageArgs.ChatInfo.Members);
            _chatView.Initialize(_chatModel);
            _postTypeSelectionPanel.Init(args.ChatInfo);
            _postTypeSelectionPanel.Hide(true);
            _mutedIcon.SetActive(args.ChatInfo.MutedUntilTime.HasValue);
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            _chatView.CleanUp();
            base.OnHidingBegin(onComplete);
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnBackButton()
        {
            if (OpenPageArgs.OpenInboxPageOnExit)
            {
                Manager.MoveNext(new InboxPageArgs());
            }
            else
            {
                Manager.MoveBack();
            }
        }

        private async void OnOptionsButton()
        {
            var result = await _bridge.GetChatById(OpenPageArgs.ChatInfo.Id, _tokenSource.Token);

            if (result.IsError)
            {
                Debug.LogError($"Failed to receive chat data, reason: {result.ErrorMessage}");
                return;
            }

            if (result.IsSuccess)
            {
                var config = new ChatSettingsPopupConfiguration(result.Model, MAX_MEMBERS, OnChatMuted);
        
                _popupManager.SetupPopup(config);
                _popupManager.ShowPopup(config.PopupType);
            }
        }

        private void OnChatMuted(bool isMuted)
        {
            _mutedIcon.SetActive(isMuted);
        }

        private void OnCreateButtonClicked()
        {
            _postTypeSelectionPanel.Show();
        }

        private void ClosePostTypeSelectionPanel()
        {
            _postTypeSelectionPanel.Hide();
        }
    }
}