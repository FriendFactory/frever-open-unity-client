using AdvancedInputFieldPlugin;
using UnityEngine;

namespace UIManaging.Common.InputFields
{
    public class AdvancedInputFieldAdapter: AdvancedInputFieldAdapterBase 
    {
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override string Text
        {
            get => InputField.Text;
            set => InputField.SetText(value, true);
        }

        public override int StringPosition { get => InputField.CaretPosition; set => InputField.CaretPosition = value; }
        public override int CaretPosition { get => InputField.CaretPosition; set => InputField.CaretPosition = value; }

        //---------------------------------------------------------------------
        // Ctors 
        //---------------------------------------------------------------------

        public AdvancedInputFieldAdapter(AdvancedInputField inputField): base(inputField) { }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public override string GetParsedText()
        {
            return InputField.Text;
        }

        public override (int, int) GetParsedTextLength(int characterLimit)
        {
            var length = 0;
            var characterLimitCaretPosition = 0;
            var text = InputField.Text;
            for (var i = 0; i < text.Length; i++)
            {
                if (NativeKeyboardManager.EmojiEngine.TryFindNextEmojiInText(text, i, out var emojiData))
                {
                    var emojiTextLength = emojiData.text.Length - 1;
                    // emojis count differently from tags
                    // we need to offset everything limit related to the emoji length, but leave length as is, i.e. 1 character
                    // we subtract one due to increment in outer loop
                    characterLimit += emojiTextLength;
                    characterLimitCaretPosition += emojiTextLength;
                    i += emojiTextLength;
                }
            }

            characterLimitCaretPosition = Mathf.Min(++characterLimitCaretPosition, characterLimit);
            length++;

            return (length, characterLimitCaretPosition);
        }
        
        public override void SetTextWithoutNotify(string input)
        {
            InputField.SetText(input, false);
        }
    }
}