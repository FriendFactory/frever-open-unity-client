using System;
using AdvancedInputFieldPlugin;
using Bridge.ClientServer.Chat;
using Bridge.Models.ClientServer.Chat;
using Modules.ContentModeration;
using UIManaging.Common.InputFields;
using UIManaging.Localization;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.PopupSystem.Popups.ChatSettings
{
    public class ChatChangeNamePopup : BasePopup<ChatChangeNamePopupConfiguration>
    {
        private const int CHARACTER_LIMIT = 25;
        
        [SerializeField] private AdvancedInputField _nameInputField;
        [SerializeField] private Button _confirmBtn;
        [SerializeField] private Button _backBtn;
        
        [Inject] private InputFieldAdapterFactory _inputFieldAdapterFactory;
        [Inject] private IChatService _bridge;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] TextContentValidator _textContentValidator;
        [Inject] private ChatLocalization _chatLocalization;
        
        private IInputFieldAdapter _inputFieldAdapter;
        
        private void OnEnable()
        {
            _confirmBtn.onClick.AddListener(OnConfirmButtonClicked);
            _backBtn.onClick.AddListener(OnBackButtonClicked);
        }

        private void OnDisable()
        {
            _confirmBtn.onClick.RemoveListener(OnConfirmButtonClicked);
            _backBtn.onClick.RemoveListener(OnBackButtonClicked);
        }

        protected override void OnConfigure(ChatChangeNamePopupConfiguration configuration)
        {
            _inputFieldAdapter = _inputFieldAdapterFactory.CreateInstance(_nameInputField);
            
            _inputFieldAdapter.OnValueChanged += OnInputValueChanged;  
            _inputFieldAdapter.CharacterLimit = CHARACTER_LIMIT;
            _inputFieldAdapter.Text = "";

            _confirmBtn.interactable = false;
        }

        protected override void OnHidden()
        {
            _inputFieldAdapter.OnValueChanged -= OnInputValueChanged;  
            _inputFieldAdapter.Dispose();
            
            base.OnHidden();
        }

        private void OnBackButtonClicked()
        {
            Hide(false);
        }

        private async void OnConfirmButtonClicked()
        {
            try
            {
                var text = _inputFieldAdapter.Text;
                var moderationPassed = await _textContentValidator.ValidateTextContent(text);

                if (!moderationPassed) return;

                var result = await _bridge.UpdateChat(Configs.ChatId, new SaveChatModel
                {
                    Title = text,
                    GroupIds = Configs.Members
                });

                if (result.IsError)
                {
                    Debug.LogError($"Failed to update chat name, reason: {result.ErrorMessage}");
                    return;
                }

                if (result.IsSuccess)
                {
                    Hide(true);
                    _snackBarHelper.ShowSuccessDarkSnackBar(_chatLocalization.RenameSuccessSnackbarMessage);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void OnInputValueChanged(string newInput)
        {
            _confirmBtn.interactable = !string.IsNullOrWhiteSpace(newInput);
        }
    }
}