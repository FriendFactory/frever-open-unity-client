using System;
using TMPro;
using UnityEngine;

namespace UIManaging.Common.InputFields
{
    public interface IInputFieldAdapter: IDisposable
    {
        event Action<string> OnValueChanged;
        event Action<KeyboardStatus> OnKeyboardStatusChanged;
        event Action<int> OnKeyboardHeightChanged;
        event Action<string> OnSubmit;
        event Action CharacterLimitReached;
        event Action OnSelect;
        event Action<string> OnDeselect;

        event Func<string, int, char, char> OnValidateInput;
        
        RectTransform Transform { get; }
        string Text { get; set; }
        float FontSize { get; set; }
        public float ResizeMaxHeight { get; set; }
        string PlaceholderText { get; set; }
        int CharacterLimit { get; set; }
        int StringPosition { get; set; }
        int CaretPosition { get; set; }
        bool Interactable { get; set; }
        bool IsFocused { get; }
        Color CaretColor { get; set; }
        TMP_InputField.InputType InputType { get; set; }
        TMP_InputField.ContentType ContentType { get; set; }

        void ActivateInputField();
        void DeactivateInputField();
        void Select();
        void Deselect();
        void SetTextWithoutNotify(string input);
        void SendOnValueChanged();
        int GetStringIndexFromCaretPosition(int caretPosition);
        int GetCaretPositionFromStringIndex(int stringIndex);
        void ForceUpdate();
        string GetParsedText();
        (int, int) GetParsedTextLength(int characterLimit);
        void InsertEmoji(string emoji);
    }
    
    public enum KeyboardStatus
    {
        Canceled,
        Done,
        Visible,
        UserDeselect
    }
}