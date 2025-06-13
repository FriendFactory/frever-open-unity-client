using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Common;
using TMPro;
using UIManaging.Common.Ui;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Utils;

namespace UIManaging.Common.InputFields
{
    public sealed class TMPInputFieldAdapter: IInputFieldAdapter
    {
        private const char OBJECT_REPLACEMENT_CHAR = '\uFFFC';

        private readonly Dictionary<TouchScreenKeyboard.Status, KeyboardStatus> _keyboardStatusMap = new Dictionary<TouchScreenKeyboard.Status, KeyboardStatus>()
        {
            {TouchScreenKeyboard.Status.Canceled, KeyboardStatus.Canceled},
            {TouchScreenKeyboard.Status.Done, KeyboardStatus.Done},
            {TouchScreenKeyboard.Status.Visible, KeyboardStatus.Visible},
        };
        
        private readonly TMP_InputField _inputField;
        private readonly FieldInfo _softKeyboardField;
        private readonly IEnumerator _keyboardHeightWatcher;
        
        private bool _wasKeyboardVisibleBefore;
        private int _maxKeyboardHeight;
        private string _currentText;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public RectTransform Transform { get; }
        
        public string Text
        {
            get => _inputField.text;
            set
            {
                _inputField.text = value;
                if (CharacterLimit > 0 && value.Length >= CharacterLimit)
                {
                    OnCharacterLimitReached();
                }
            }
        }

        public float FontSize
        {
            get => _inputField.textComponent.fontSize;
            set => _inputField.textComponent.fontSize = value;
        }

        public float ResizeMaxHeight { get; set; }

        public string PlaceholderText { get => _inputField.placeholder.GetComponent<TMP_Text>().text; set => _inputField.placeholder.GetComponent<TMP_Text>().text = value; }
        
        //Add 1 extra to limit count in order to trigger OnValueChanged each time a character is pressed and then we remove last character.
        public int CharacterLimit { get => _inputField.characterLimit; set => _inputField.characterLimit = value + 1; }
        public int StringPosition { get => _inputField.stringPosition; set => _inputField.stringPosition = value; }
        public int CaretPosition { get => _inputField.caretPosition; set => _inputField.caretPosition = value; }
        public bool Interactable { get => _inputField.interactable; set => _inputField.interactable = value; }
        public bool IsFocused => _inputField.isFocused;
        public Color CaretColor { get => _inputField.caretColor; set => _inputField.caretColor = value; }
        public TMP_InputField.InputType InputType { get => _inputField.inputType; set => _inputField.inputType = value; }
        public TMP_InputField.ContentType ContentType { get => _inputField.contentType; set => _inputField.contentType = value; }
        private int KeyboardHeight => MobileUtilities.GetKeyboardHeight(!_inputField.shouldHideMobileInput);

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<string> OnValueChanged;
        public event Action<string> OnSubmit;
        public event Action OnSelect;
        public event Action<string> OnDeselect;
        public event Action<KeyboardStatus> OnKeyboardStatusChanged;
        public event Action<int> OnKeyboardHeightChanged;
        public event Action CharacterLimitReached;
        
        public event Func<string, int, char, char> OnValidateInput;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public TMPInputFieldAdapter(TMP_InputField inputField, SnackBarHelper snackBarHelper)
        {
            Transform = (RectTransform) inputField.transform;
            _inputField = inputField;

            _inputField.onValueChanged.AddListener(OnInputValueChanged);
            _inputField.onSubmit.AddListener(Submit);
            _inputField.onTouchScreenKeyboardStatusChanged.AddListener(OnTouchScreenKeyboardStatusChanged);
            _inputField.onDeselect.AddListener(Deselect);
            _inputField.onValidateInput += OnValidateInputWrapper;
            if(_inputField.characterValidation == TMP_InputField.CharacterValidation.None) _inputField.onValidateInput += ValidateCharacter;

            _softKeyboardField = _inputField.GetType().GetField("m_SoftKeyboard", BindingFlags.Instance | BindingFlags.NonPublic);

            _keyboardHeightWatcher = KeyboardHeightWatcher();
            CoroutineSource.Instance.StartCoroutine(_keyboardHeightWatcher);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void ActivateInputField()
        {
            _inputField.ActivateInputField();
            OnSelect?.Invoke();
        }

        public void DeactivateInputField()
        {
            _inputField.DeactivateInputField();
        }

        public void Select()
        {
            _inputField.Select();
            OnSelect?.Invoke();
        }

        public void Deselect()
        {
            _inputField.DeactivateInputField();
        }

        public void SetTextWithoutNotify(string input)
        {
            _inputField.SetTextWithoutNotify(input);
        }

        public void SendOnValueChanged()
        {
            _inputField.onValueChanged?.Invoke(Text);
        }

        public int GetStringIndexFromCaretPosition(int caretPosition)
        {
            _inputField.ForceLabelUpdate();
            _inputField.textComponent.ForceMeshUpdate();
            // Clamp values between 0 and character count.
            ClampCaretPos(_inputField, ref caretPosition);

            return _inputField.textComponent.textInfo.characterInfo[caretPosition].index;
        }

        public int GetCaretPositionFromStringIndex(int stringIndex)
        {
            _inputField.ForceLabelUpdate();
            _inputField.textComponent.ForceMeshUpdate();

            var textComponent = _inputField.textComponent;
            var count = textComponent.textInfo.characterCount;

            for (var i = 0; i < count; i++)
            {
                if (textComponent.textInfo.characterInfo[i].index >= stringIndex) return i;
            }

            return count;
        }

        public void ForceUpdate()
        {
            _inputField.ForceLabelUpdate();
            _inputField.textComponent.ForceMeshUpdate();
        }

        public string GetParsedText()
        {
            var text = Regex.Replace(Text, MentionsPanel.MENTION_REGEX, MentionsPanel.MENTION_ID_PATTERN);
            text = AdvancedInputFieldUtils.GetParsedText(text);
            return text;
        }

        public (int, int) GetParsedTextLength(int characterLimit)
        {
            var parsedText = GetParsedText();
            var characterLimitCaretPosition = GetCaretPositionFromStringIndex(characterLimit);
            var lenght = 0;

            for (var i = 0; i < parsedText.Length; i++)
            {
                // emojis encoded as UTF32 or two UTF16 chars so let's count them as one character
                if (char.IsSurrogate(parsedText, i)) i++;
                lenght++;
            }

            return (lenght, characterLimitCaretPosition);
        }

        public void InsertEmoji(string emoji)
        {
            _inputField.text = _inputField.text.Insert(_inputField.caretPosition, emoji);
            _inputField.caretPosition += emoji.Length;
            _inputField.Select();
        }

        public void Dispose()
        {
            _inputField.onValueChanged.RemoveListener(OnInputValueChanged);
            _inputField.onSubmit.RemoveListener(Submit);
            _inputField.onTouchScreenKeyboardStatusChanged.RemoveListener(OnTouchScreenKeyboardStatusChanged);
            _inputField.onValidateInput -= OnValidateInputWrapper;
            _inputField.onValidateInput -= ValidateCharacter;

            if (_keyboardHeightWatcher != null)
            {
                CoroutineSource.Instance.StopCoroutine(_keyboardHeightWatcher);
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnTouchScreenKeyboardStatusChanged(TouchScreenKeyboard.Status status)
        {
            switch (status)
            {
                case TouchScreenKeyboard.Status.Visible:
                case TouchScreenKeyboard.Status.LostFocus:
                case TouchScreenKeyboard.Status.Done:
                    ProcessStatus();
                    break;
                case TouchScreenKeyboard.Status.Canceled:
                    // TMP_InputField issue when canceling iOS keyboard
                    CoroutineSource.Instance.ExecuteAtEndOfFrame(ProcessStatus);
                    break;
            }
            
            void ProcessStatus()
            {
                if (_keyboardStatusMap.ContainsKey(status))
                {
                    OnKeyboardStatusChanged?.Invoke(_keyboardStatusMap[status]);
                }
            }
        }

        private void OnInputValueChanged(string value)
        {
            if (CharacterLimit > 0 && _inputField.text.Length >= CharacterLimit)
            {
                OnCharacterLimitReached();
                return;
            }

            _currentText = value;
            OnValueChanged?.Invoke(_currentText);
        }

        private char OnValidateInputWrapper(string text, int index, char addedChar)
        {
            return OnValidateInput?.Invoke(text, index, addedChar) ?? addedChar;
        }

        private void OnCharacterLimitReached()
        {
            _inputField.SetTextWithoutNotify(_currentText);
            _inputField.MoveTextEnd(false);
            CharacterLimitReached?.Invoke();
        }

        private void Submit(string text)
        {
            OnSubmit?.Invoke(text);
        }
        
        private void Deselect(string text)
        {
            OnDeselect?.Invoke(text);
        }

        private static void ClampCaretPos(TMP_InputField inputField, ref int pos)
        {
            if (pos < 0)
            {
                pos = 0;
            }
            else if (pos > inputField.textComponent.textInfo.characterCount - 1)
            {
                pos = inputField.textComponent.textInfo.characterCount - 1;
            }
        }

        //---------------------------------------------------------------------
        // Coroutines
        //---------------------------------------------------------------------

        private IEnumerator KeyboardHeightWatcher()
        {
            while (true)
            {
                var isKeyboardVisible = IsKeyboardVisible();
                if (isKeyboardVisible != _wasKeyboardVisibleBefore)
                {
                    if (isKeyboardVisible)
                    {
                        yield return new WaitUntil(() => KeyboardHeight > 0);
                    }

                    _wasKeyboardVisibleBefore = isKeyboardVisible;
                    _maxKeyboardHeight = Mathf.Max(_maxKeyboardHeight, KeyboardHeight);
                    var height = isKeyboardVisible ? _maxKeyboardHeight : 0;

                    OnKeyboardHeightChanged?.Invoke(height);
                }

                yield return null;
            }
        }

        private char ValidateCharacter(string text, int pos, char newCharacter)
        {
            newCharacter = newCharacter == OBJECT_REPLACEMENT_CHAR ? ' ' : newCharacter;
            return newCharacter;
        }

        //---------------------------------------------------------------------
        // Extensions
        //---------------------------------------------------------------------

        private bool IsKeyboardVisible()
        {
            var softKeyboard = (TouchScreenKeyboard) _softKeyboardField.GetValue(_inputField);
            return softKeyboard?.status == TouchScreenKeyboard.Status.Visible;
        }
    }
}