using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer;
using Bridge.Services.UserProfile;
using TMPro;
using UIManaging.Common.InputFields;
using UIManaging.Common.SearchPanel;
using UIManaging.Localization;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Common.Ui
{
    public sealed class MentionsPanel : MonoBehaviour
    {
        public const string MENTION_REGEX = "<link=\"mention:([0-9]+)\">\\s*(.+?)\\s*</link>";
        public const string MENTION_ID_PATTERN = "@$1";
        public const string MENTION_TAG = "<link=\"mention:{0}\"><style=\"Mention\">{1}</style></link>";

        [SerializeField] private Button _backButton;
        [SerializeField] private SearchPanelView _searchPanelView;
        [SerializeField] private SearchHandler _searchHandler;
        [SerializeField] private bool _handleCharacterLimit = false;

        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private MentionsPanelLocalization _localization;
        
        private IInputFieldAdapter _inputFieldAdapter;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public Action<(string, string)> OnProfileSelected;
        public Action OnBackButtonPressed;
        public Action OnPanelShown;
        public Action OnPanelHidden;
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            _backButton.onClick.AddListener(OnBackButtonClick);
            _searchHandler.ProfileButtonClicked += OnProfileButtonClicked;
        }
        
        private void OnDisable()
        {
            _backButton.onClick.RemoveListener(OnBackButtonClick);
            _searchHandler.ProfileButtonClicked -= OnProfileButtonClicked;
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(IInputFieldAdapter inputFieldAdapter)
        {
            _inputFieldAdapter = inputFieldAdapter;
        }
        
        public void Show()
        {
            gameObject.SetActive(true);
            _searchHandler.SetSearchHandling(true);
            _searchPanelView.Select();
            OnPanelShown?.Invoke();
        }

        public void Hide()
        {
            if (!_inputFieldAdapter.IsFocused)
            {
                _inputFieldAdapter.Select();
                _inputFieldAdapter.ActivateInputField();
            }
            _searchHandler.SetSearchHandling(false);
            _searchPanelView.Clear();

            gameObject.SetActive(false);
            OnPanelHidden?.Invoke();
        }

        public void SetUserList(IEnumerable<GroupShortInfo> users)
        {
            _searchHandler.SetUserList(users);
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnBackButtonClick()
        {
#if ADVANCEDINPUTFIELD_TEXTMESHPRO 
            OnBackButtonPressed?.Invoke();
#else
            Hide();
#endif
        }
        
        private void OnProfileButtonClicked(Profile profile)
        {
            _inputFieldAdapter.ForceUpdate();

            var text = _inputFieldAdapter.Text;
            
            var nickName = AdvancedInputFieldUtils.GetParsedText(profile.NickName);
            var mentionText = string.Format(MENTION_TAG, profile.MainGroupId, $"@{nickName}");

            if (text.Contains(mentionText))
            {
                _snackBarHelper.ShowInformationSnackBar(_localization.UserAlreadyMentionedSnackbarMessage, 2);
                return;
            }
#if ADVANCEDINPUTFIELD_TEXTMESHPRO 
            var mentionLength = nickName.Length + 1;
            var (parsedLength, _) = _inputFieldAdapter.GetParsedTextLength(_inputFieldAdapter.CharacterLimit);
            if (!_handleCharacterLimit
             || _inputFieldAdapter.CharacterLimit >= parsedLength + mentionLength)
            {
                OnProfileSelected?.Invoke((nickName, mentionText));
            }
            else
            {
                _inputFieldAdapter.Text = text.Remove(_inputFieldAdapter.Text.Length - 1);
                
                _snackBarHelper.ShowInformationSnackBar("Reached text limitation", 2);
                Hide();
            }
#else 
            var caretPosition = InputFieldAdapter.CaretPosition;
            var stringPosition = InputFieldAdapter.StringPosition;
            var mentionLenght = nickName.Length + 1;

            // Check if `@` already exists before caret position
            if (stringPosition > 0 && text[stringPosition - 1].Equals('@'))
            {
                text = text.Remove(stringPosition - 1, 1);
                caretPosition -= 1;
                stringPosition -= 1;
            }

            // Check if space already exists before caret position
            if (stringPosition > 0 && !text[stringPosition - 1].Equals(' '))
            {
                mentionText = $" {mentionText}";
                mentionLenght += 1;
            }

            // Check if space already exists after caret position
            if (stringPosition == text.Length || !text[stringPosition].Equals(' '))
            {
                mentionText = $"{mentionText} ";
                mentionLenght += 1;
            }

            if (!_handleCharacterLimit
             || _inputFieldAdapter.CharacterLimit >= _inputFieldAdapter.Text.Length + mentionLenght)
            {
                _inputFieldAdapter.Text = text.Insert(stringPosition, mentionText);
                _inputFieldAdapter.StringPosition = stringPosition + mentionText.Length;
                _inputFieldAdapter.CaretPosition = caretPosition + mentionLenght;
            }
            else
            {
                _snackBarHelper.ShowInformationSnackBar("Reached text limitation", 2);
            }

            Hide();
#endif
        }
    }
}