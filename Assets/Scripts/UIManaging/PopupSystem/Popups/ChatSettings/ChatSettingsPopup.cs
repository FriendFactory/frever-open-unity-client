using System.Threading;
using Bridge.ClientServer.Chat;
using Extensions;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Localization;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using ReportMessageBridgeModel = Bridge.Models.ClientServer.Chat.ReportMessageModel;

namespace UIManaging.PopupSystem.Popups.ChatSettings
{
    public class ChatSettingsPopup : BasePopup<ChatSettingsPopupConfiguration>
    {
        private const int CONVERSATION_REASON = 1;
        
        [SerializeField] private ChatSettingsPanel _settingsPanel;
        [SerializeField] private Button _backBtn;

        [Inject] private PageManager _pageManager;
        [Inject] private PopupManager _popupManager;
        [Inject] private IChatService _bridge;
        [Inject] private PopupManagerHelper _popupHelper;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private ChatLocalization _localization;

        private CancellationTokenSource _tokenSource;

        private void OnEnable()
        {
            _backBtn.onClick.AddListener(Hide);
            
            _settingsPanel.HideActionRequested += Hide;
            _settingsPanel.ReportActionRequested += OnReportRequested;
            _settingsPanel.DeleteActionRequested += DeleteAction;
            _settingsPanel.GoToChatActionRequested += GoToChatAction;

            _tokenSource = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            _tokenSource.CancelAndDispose();
            
            _settingsPanel.HideActionRequested -= Hide;
            _settingsPanel.ReportActionRequested -= OnReportRequested;
            _settingsPanel.DeleteActionRequested -= DeleteAction;
            _settingsPanel.GoToChatActionRequested -= GoToChatAction;
            
            _backBtn.onClick.RemoveListener(Hide);
        }

        protected override void OnConfigure(ChatSettingsPopupConfiguration configuration)
        {
            _settingsPanel.Initialize(new ChatSettingsModel(configuration.ChatInfo, configuration.MaxMembers, configuration.OnMuted));
        }

        protected override void OnHidden()
        {
            _settingsPanel.CleanUp();
            
            base.OnHidden();
        }

        private void OnReportRequested()
        {
            var confirmPopupConfiguration = new DialogDarkPopupConfiguration
            {
                PopupType = PopupType.DialogDark,
                Title = _localization.ReportPopupTitle,
                Description = _localization.ReportPopupDescription,
                YesButtonText = _localization.ReportPopupConfirmButton,
                NoButtonText = _localization.ReportPopupCancelButton,
                OnYes = Report
            };

            _popupManager.SetupPopup(confirmPopupConfiguration);
            _popupManager.ShowPopup(confirmPopupConfiguration.PopupType, true);
        }

        private async void Report()
        {
            if (!Configs.ChatInfo.LastReadMessageId.HasValue)
            {
                _snackBarHelper.ShowFailSnackBar(_localization.CantReportEmptyChatSnackbarMessage);
                return;
            }
            
            var model = new ReportMessageBridgeModel
            {
                ChatMessageId = Configs.ChatInfo.LastReadMessageId.Value,
                ReasonId = CONVERSATION_REASON,
            };

            var result = await _bridge.ReportMessage(Configs.ChatInfo.Id, model);

            if (result.IsSuccess)
            {
                Hide();
                _pageManager.MoveNext(new ChatPageArgs(Configs.ChatInfo), false);
                _popupHelper.ShowDialogPopup(_localization.ReportSuccessPopupTitle, _localization.ReportSuccessPopupDescription, null, null,
                                                    _localization.ReportSuccessPopupConfirmButton, CloseSuccessPopup, false);
            }

            if (result.IsError)
            {
                Debug.LogError(result.ErrorMessage);
            }
            
            void CloseSuccessPopup()
            {
                _popupManager.ClosePopupByType(PopupType.Dialog);
            }
        }

        private void DeleteAction()
        {
            var config = new DialogDarkPopupConfiguration
            {
                PopupType = PopupType.DialogDarkV3,
                Title = _localization.LeaveGroupPopupTitle,
                Description =_localization.LeaveGroupPopupDescription,
                YesButtonText = _localization.LeaveGroupPopupConfirmButton,
                NoButtonText =  _localization.LeaveGroupPopupCancelButton,
                YesButtonSetTextColorRed = true,
                OnYes = LeaveChat,
            };
            
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType, true);
        }
        
        private async void LeaveChat()
        {
            var result = await _bridge.LeaveChat(Configs.ChatInfo.Id);
            
            if (result.IsSuccess)
            {
                Hide();
                _pageManager.MoveNext(new InboxPageArgs());
                return;
            }

            if (result.IsError)
            {
                Debug.LogError(result.ErrorMessage);
            }
        }

        private async void GoToChatAction(long chatId)
        {
            var result = await _bridge.GetChatById(chatId, _tokenSource.Token);

            if (result.IsError)
            {
                Debug.LogError($"Failed to receive chat data, reason: {result.ErrorMessage}");
                return;
            }

            if (result.IsSuccess)
            {
                _pageManager.MoveNext(new ChatPageArgs(result.Model), false);
                Hide();
            }
        }
    }
}