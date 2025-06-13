using System;
using System.Collections.Generic;
using System.Reflection;
using AdvancedInputFieldPlugin;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Common.InputFields
{
    public abstract class AdvancedInputFieldAdapterBase: IInputFieldAdapter
    {
        private PropertyInfo _engineProp;
        protected readonly AdvancedInputField InputField;

        private readonly Dictionary<EndEditReason, KeyboardStatus> _keyboardEndReasonStatusMap = new Dictionary<EndEditReason, KeyboardStatus>()
        {
            { EndEditReason.KEYBOARD_CANCEL, KeyboardStatus.Canceled},
            { EndEditReason.KEYBOARD_DONE, KeyboardStatus.Done},
            { EndEditReason.USER_DESELECT, KeyboardStatus.UserDeselect},
        };
        
        private readonly Dictionary<BeginEditReason, KeyboardStatus> _keyboardBeginReasonStatusMap = new Dictionary<BeginEditReason, KeyboardStatus>()
        {
            { BeginEditReason.USER_SELECT, KeyboardStatus.Visible},
        };
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public RectTransform Transform { get; }

        public float FontSize
        {
            get => InputField.GetComponentInChildren<TMP_Text>().fontSize;
            set 
            {
                var texts = InputField.GetComponentsInChildren<TMP_Text>();
                foreach (var text in texts)
                {
                    text.fontSize = value;
                }
                
                UpdateCaretAndCursorPositions();
            }
        }

        public float ResizeMaxHeight
        {
            get => InputField.ResizeMaxHeight;
            set
            {
                if (Mathf.Approximately(InputField.ResizeMaxHeight, value)) return;
                
                InputField.ResizeMaxHeight = value;
                
                // two times hacky, but works
                InputField.Engine.UpdateActiveTextRenderer();
                
                LayoutRebuilder.ForceRebuildLayoutImmediate(Transform.root as RectTransform);
            }
        }

        public string PlaceholderText { get => InputField.PlaceHolderText; set => InputField.PlaceHolderText = value; }
        //Add 1 extra to limit count in order to trigger OnValueChanged each time a character is pressed and then we remove last character.
        public int CharacterLimit { get => InputField.CharacterLimit; set => InputField.CharacterLimit = value + 1; }
        public bool Interactable { get => InputField.interactable; set => InputField.interactable = value; }
        public bool IsFocused => InputField.Selected;
        public Color CaretColor { get => InputField.CaretColor; set => InputField.CaretColor = value; }
        public TMP_InputField.InputType InputType { get => (TMP_InputField.InputType)(int)InputField.InputType; set => InputField.InputType = (InputType)(int)value; }
        public TMP_InputField.ContentType ContentType { get => (TMP_InputField.ContentType)(int)InputField.ContentType; set => InputField.ContentType = (ContentType)(int)value; }
        
        public abstract string Text { get; set; }
        public abstract int StringPosition { get; set; }
        public abstract int CaretPosition { get; set; }

        private PropertyInfo EnginePropertyInfo
        {
            get
            {
                if (_engineProp == null)
                {
                    _engineProp = InputField.GetType().GetProperty("Engine", BindingFlags.NonPublic | BindingFlags.Instance);
                }
                return _engineProp;
            }
        }
        //---------------------------------------------------------------------
        //Events 
        //---------------------------------------------------------------------

        public event Action<string> OnValueChanged;
        public event Action<KeyboardStatus> OnKeyboardStatusChanged;
        public event Action<int> OnKeyboardHeightChanged;
        #pragma warning disable 0067
        public event Action<string> OnSubmit;
        public event Action CharacterLimitReached;
        public event Action OnSelect;
        public event Action<string> OnDeselect;
        public event Func<string, int, char, char> OnValidateInput;
        #pragma warning restore 0067
        
        //---------------------------------------------------------------------
        // Ctors 
        //---------------------------------------------------------------------

        protected AdvancedInputFieldAdapterBase(AdvancedInputField inputField)
        {
            if (inputField == null) throw new ArgumentNullException($"[{GetType().Name}] Provided input field could not be null");
            
            InputField = inputField;
            Transform = (RectTransform) InputField.transform;
            InputField.OnValueChanged.AddListener(OnInputValueChanged);
            InputField.OnEndEdit.AddListener(OnEndEdit);
            InputField.OnBeginEdit.AddListener(OnBeginEdit);

            NativeKeyboardManager.AddKeyboardHeightChangedListener(KeyboardHeightChanged);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void ActivateInputField()
        {
            Select();
        }

        public void DeactivateInputField()
        {
            Deselect();
            OnDeselect?.Invoke(Text);
        }

        public void Select()
        {
            InputField.ManualSelect();
            OnSelect?.Invoke();
        }

        public void SendOnValueChanged()
        {
            InputField.OnValueChanged.Invoke(Text);
        }

        public virtual int GetStringIndexFromCaretPosition(int caretPosition)
        {
            var characterInfo = InputField.TextRenderer.GetCharacterInfo(caretPosition);
            return characterInfo.index;
        }

        public virtual int GetCaretPositionFromStringIndex(int stringIndex)
        {
            return InputField.DeterminePositionInRichText(stringIndex);
        }

        public void ForceUpdate() { }

        public void Deselect()
        {
            InputField.ShouldBlockDeselect = false;
            InputField.ManualDeselect();
        }
        
        public virtual void Dispose()
        {
            InputField.OnValueChanged.RemoveListener(OnInputValueChanged);
            InputField.OnEndEdit.RemoveListener(OnEndEdit);
            
            NativeKeyboardManager.RemoveKeyboardHeightChangedListener(KeyboardHeightChanged);
        }
        
        public virtual void InsertEmoji(string emoji)
        {
            var caretPosition = InputField.CaretPosition;
            var text = InputField.Text.Insert(InputField.CaretPosition, emoji);

            InputField.SetText(text, true);
            InputField.CaretPosition = caretPosition + emoji.Length;
        }
        
        public abstract string GetParsedText();
        public abstract (int, int) GetParsedTextLength(int characterLimit);
        public abstract void SetTextWithoutNotify(string input);
        
        //---------------------------------------------------------------------
        // Helpers 
        //---------------------------------------------------------------------
        
        private void OnBeginEdit(BeginEditReason reason)
        {
            if (!_keyboardBeginReasonStatusMap.ContainsKey(reason)) return;
            
            OnKeyboardStatusChanged?.Invoke(_keyboardBeginReasonStatusMap[reason]);
        }

        private void OnEndEdit(string value, EndEditReason reason)
        {
            if (!_keyboardEndReasonStatusMap.ContainsKey(reason)) return;
            
            if (reason == EndEditReason.KEYBOARD_DONE)
            {
                OnSubmit?.Invoke(Text);
            }
            
            OnKeyboardStatusChanged?.Invoke(_keyboardEndReasonStatusMap[reason]);
        }
        
        private void OnInputValueChanged(string value)
        {
            OnValueChanged?.Invoke(value);
        }

        private void KeyboardHeightChanged(int height)
        {
            OnKeyboardHeightChanged?.Invoke(height);
        }
        
        private void UpdateCaretAndCursorPositions()
        {
            //workaround
            var engine = EnginePropertyInfo?.GetValue(InputField) as InputFieldEngine;
            engine?.MarkDirty();
        }
    }
}