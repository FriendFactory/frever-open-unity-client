using System.Collections;
using System.Linq;
using AdvancedInputFieldPlugin;
using UnityEngine;

namespace UIManaging.Pages.SharingPage.Ui
{
    public struct Occurrence
    {
        public string text;
        public int wordStartIndex;
        public int wordEndIndex;
        public int caretPositionOffset;
    }

    [RequireComponent(typeof(AdvancedInputField))]
    public abstract class DescriptionHandlerBase<T>: MonoBehaviour
    {
        protected abstract char StartWith { get; }
        protected virtual bool IsTerminating(char @char) => @char == ' ' || @char == '\n';

        protected AdvancedInputField InputField =>
            _inputField ? _inputField : _inputField = GetComponent<AdvancedInputField>();

        private AdvancedInputField _inputField;
        private int _wordStartIndex;
        private int _wordEndIndex;
        private int _caretPositionOffset;

        public bool TryFindOccurrence(TextEditFrame textEditFrame, out Occurrence occurrence)
        {
            occurrence = new Occurrence();

            if (textEditFrame.selectionStartPosition != textEditFrame.selectionEndPosition) return false;

            var text = textEditFrame.text.ToLower();
            var caretPosition = textEditFrame.selectionStartPosition;
            var startIndex = -1;

            for (var i = caretPosition - 1; i >= 0; i--)
            {
                var current = text[i];
                if (current == StartWith)
                {
                    var nextIndex = i + 1;
                    if (nextIndex <= caretPosition - 1)
                    {
                        var next = text[i + 1];
                        startIndex = char.IsLetterOrDigit(next) ? nextIndex : -1;
                    }
                    else
                    {
                        startIndex = nextIndex;
                    }

                    break;
                }

                if (IsTerminating(current))
                {
                    break;
                }
            }

            if (startIndex == -1) return false;

            var amount = 0;
            var length = Mathf.Min(text.Length, caretPosition);
            var caretPositionOffset = 0;
            for (var i = startIndex; i < length; i++)
            {
                var c = text[i];
                // terminate occurence if non-allowed character has found
                if (!char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c))
                {
                    // insert space as a marker for tag autocompletion
                    text = text.Insert(startIndex + amount, " ");
                    textEditFrame.text = text;
                    caretPositionOffset += 1;
                    amount += 1;
                    break;
                }
                //Just quit when you encounter a space, emoji, or newline character
                if (IsTerminating(c) || char.IsSurrogate(c) && NativeKeyboardManager.EmojiEngine.IsEmoji(c))
                {
                    break;
                }

                amount++;
            }

            occurrence.text = text;
            occurrence.wordStartIndex = startIndex - 1;
            occurrence.wordEndIndex = startIndex + amount;
            occurrence.caretPositionOffset = caretPositionOffset;

            return true;
        }

        public virtual bool ValidateConstraints(ref TextEditFrame textEditFrame) => true;
        
        public virtual void Process(Occurrence occurrence)
        {
            _wordStartIndex = occurrence.wordStartIndex;
            _wordEndIndex = occurrence.wordEndIndex;
            _caretPositionOffset = occurrence.caretPositionOffset;
        }

        protected abstract RichTextBindingData GetRichTextBindingData(T item);
        public abstract void Hide();

        protected virtual void OnItemSelected(T item)
        {
            var tagData = GetRichTextBindingData(item);
            var hasBinding = NativeKeyboardManager.RichTextBindingEngine.TryGetBindingFromName(tagData.name, out var _);
            if (!hasBinding)
            {
                NativeKeyboardManager.RichTextBindingEngine.AddBinding(tagData);
            }

            StartCoroutine(ApplyTag(tagData.name));
        }

        private IEnumerator ApplyTag(string tagName)
        {
            yield return null;

            var hasBinding = NativeKeyboardManager.RichTextBindingEngine.TryGetBindingFromName(tagName, out var tagData);
            if (hasBinding)
            {
			    InputField.SetTextSelection(_wordStartIndex, _wordEndIndex - _caretPositionOffset);
                InputField.ReplaceSelectedTextInRichText($"{tagData.codePoint.ToString()} ");
                InputField.CaretPosition += _caretPositionOffset;
            }

            InputField.ShouldBlockDeselect = false;
            
            Hide();
        }
    }
}