using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abstract;
using AdvancedInputFieldPlugin;
using Bridge;
using Bridge.Models.ClientServer.Chat;
using Extensions;
using UIManaging.Common.Dropdown;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Dropdown = UIManaging.Common.Dropdown.Dropdown;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class ReportMessageModel
    {
        public readonly long ChatId;
        public readonly long MessageId;
        
        public ReportMessageModel(long chatId, long messageId)
        {
            ChatId = chatId;
            MessageId = messageId;
        }
    }

    internal sealed class ReportMessagePanel : BaseContextDataView<ReportMessageModel>
    {
        private const string SUCCESS_TITLE = "Report successful";
        private const string SUCCESS_MESSAGE = "Thank you for your feedback! Our content team will review your report.";
        
        [SerializeField] private Button _backButton;

        [Space]
        [SerializeField] private Dropdown _reasonDropdown;
        [SerializeField] private AdvancedInputField _inputField;
        [SerializeField] private Button _reportButton;
        
        [Inject] private IBridge _bridge;
        [Inject] private PopupManager _popupManager;
        [Inject] private PopupManagerHelper _popupManagerHelper;

        private ReportReason[] _reasons;
        private CancellationTokenSource _tokenSource;

        public event Action HideActionRequested;

        private void OnEnable()
        {
            _reasonDropdown.OnOptionSelected += OnReasonSelected;

            _reportButton.interactable = false;
            _reportButton.onClick.AddListener(ReportAction);
            _backButton.onClick.AddListener(OnBackButtonClicked);
        }

        private void OnDisable()
        {
            _inputField.Text = string.Empty;
            _tokenSource?.CancelAndDispose();
            _tokenSource = null;
            
            _reportButton.onClick.RemoveAllListeners();
            _backButton.onClick.RemoveAllListeners();
        }

        protected override async void OnInitialized()
        {
            _tokenSource = new CancellationTokenSource();

            _reasons = await FetchReportReasons();
            var filteredReasonNames = _reasons.Where(r => r.Id != 1).Select(r => r.Name);

            var model = new DropdownModel(filteredReasonNames.ToList());
            _reasonDropdown.Initialize(model);
        }

        private void OnBackButtonClicked()
        {
            HideActionRequested?.Invoke();
        }

        private async Task<ReportReason[]> FetchReportReasons()
        {
            var result = await _bridge.GetMessageReportReasons(_tokenSource.Token);
            if (result.IsSuccess)
            {
                return result.Models;
            }

            if (result.IsError)
            {
                Debug.LogError(result.ErrorMessage);
            }

            return null;
        }
        
        private void OnReasonSelected(int selectedOption)
        {
            _reportButton.interactable = selectedOption != -1;
        }

        private async void ReportAction()
        {
            // Workaround: Incrementing value here to match backend indexing because first option is hidden
            var reasonId = _reasons[_reasonDropdown.SelectedOption].Id + 1;
            var description = _inputField.Text;

            var model = new Bridge.Models.ClientServer.Chat.ReportMessageModel
            {
                ChatMessageId = ContextData.MessageId,
                ReasonId = reasonId,
                ReportText = string.IsNullOrWhiteSpace(description) ? null : description,
            };

            var result = await _bridge.ReportMessage(ContextData.ChatId, model);
            if (result.IsSuccess)
            {
                HideActionRequested?.Invoke();
                _popupManagerHelper.ShowDialogPopup(SUCCESS_TITLE, SUCCESS_MESSAGE, null, null, 
                    "Ok", CloseSuccessPopup, false);
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
    }
}