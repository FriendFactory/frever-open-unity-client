using System;
using AdvancedInputFieldPlugin;
using TMPro;
using UIManaging.Common.InputFields;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.SharingPage.Ui
{
    internal sealed class DescriptionLimitHandler
    {
        private const string LIMIT_START_MARK = "<mark=#FF000050>";
        private const string LIMIT_END_MARK = "</mark>";

        private readonly IInputFieldAdapter _inputFieldAdapter;
        private readonly int _characterLimit;
        private readonly Text _characterLimitText;
        private readonly Color _characterLimitNormal;
        private readonly Color _characterLimitExceeded;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public bool IsCharLimitExceeded { get; private set; }
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<bool> CharacterLimitExceededStatusChanged;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public DescriptionLimitHandler(IInputFieldAdapter inputFieldAdapter,
                                       int characterLimit, Text characterLimitText,
                                       Color characterLimitNormal, Color characterLimitExceeded)
        {
            _inputFieldAdapter = (AdvancedInputFieldRichTextAdapter)inputFieldAdapter;

            _characterLimit = characterLimit;
            _characterLimitText = characterLimitText;
            _characterLimitNormal = characterLimitNormal;
            _characterLimitExceeded = characterLimitExceeded;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void HandleCharacterLimit()
        {
            CheckCharacterLimit();
            UpdateCharacterLimitText();
        }

        public void UpdateCharacterLimitText()
        {
            var (parsedLength, _) = _inputFieldAdapter.GetParsedTextLength(_characterLimit);
            var currentLimit = _characterLimit - parsedLength;
            _characterLimitText.text = currentLimit.ToString();
            _characterLimitText.color = (currentLimit >= 0) ? _characterLimitNormal : _characterLimitExceeded;
        }

        public void StripMarkTags()
        {
            var text = _inputFieldAdapter.Text
                            .Replace(LIMIT_START_MARK, string.Empty)
                            .Replace(LIMIT_END_MARK, string.Empty);

            if (_inputFieldAdapter.Text == text) return;

            var caretPosition = _inputFieldAdapter.CaretPosition;
            _inputFieldAdapter.SetTextWithoutNotify(text);
            _inputFieldAdapter.StringPosition = _inputFieldAdapter.GetStringIndexFromCaretPosition(caretPosition);
            _inputFieldAdapter.ForceUpdate();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void CheckCharacterLimit()
        {
            var length = _inputFieldAdapter.Text.Length;
            var (parsedLength, characterLimitCaretPosition) = _inputFieldAdapter.GetParsedTextLength(_characterLimit);
            var limitExceeded = IsCharLimitExceeded;
            IsCharLimitExceeded = length > 0 && parsedLength > _characterLimit;
            
            if (limitExceeded != IsCharLimitExceeded)
            {
                CharacterLimitExceededStatusChanged?.Invoke(IsCharLimitExceeded);
            }

            if (!IsCharLimitExceeded) return;

            _inputFieldAdapter.ForceUpdate();
            var stringIndex = _inputFieldAdapter.StringPosition;
            var caretPosition = _inputFieldAdapter.CaretPosition;
            AddMarkTags(caretPosition, characterLimitCaretPosition);
        }

        private void AddMarkTags(int caretPosition, int characterLimitCaretPosition)
        {
            var charLimitIndex = _inputFieldAdapter.GetCaretPositionFromStringIndex(characterLimitCaretPosition);
            var text = _inputFieldAdapter.Text.Insert(charLimitIndex, LIMIT_START_MARK) + LIMIT_END_MARK;

            _inputFieldAdapter.SetTextWithoutNotify(text);
            _inputFieldAdapter.CaretPosition = caretPosition;
#if !ADVANCEDINPUTFIELD_TEXTMESHPRO
            _inputFieldAdapter.StringPosition = _inputFieldAdapter.GetStringIndexFromCaretPosition(caretPosition);
#endif
        }
    }
}