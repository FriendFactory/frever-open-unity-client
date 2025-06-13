using System.Text.RegularExpressions;
using AdvancedInputFieldPlugin;
using TMPro;
using UIManaging.Common.Ui;

namespace UIManaging.Common.InputFields
{
    public class AdvancedInputFieldRichTextAdapter: AdvancedInputFieldAdapterBase 
    {
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override string Text
        {
            get => InputField.RichText;
            set
            {
                InputField.SetRichText(value);
                InputField.OnValueChanged?.Invoke(value);
            }
        }

        public override int StringPosition { get => InputField.DeterminePositionInRichText(InputField.CaretPosition); set => InputField.CaretPosition = value; }
        public override int CaretPosition { get => InputField.CaretPosition; set => InputField.CaretPosition = value; }
        
        //---------------------------------------------------------------------
        // Ctors 
        //---------------------------------------------------------------------

        public AdvancedInputFieldRichTextAdapter(AdvancedInputField inputField) : base(inputField) { }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override string GetParsedText()
        {
            var text = InputField.Text;
            var richText = InputField.RichText;

            var offset = 0;
            for (var i = 0; i < text.Length; i++)
            {
                if (!NativeKeyboardManager.EmojiEngine.TryFindNextEmojiInText(text, i, out var emojiData)) continue;
                
                var start = InputField.DeterminePositionInRichText(i);
                var end = start + emojiData.richText.Length;
                    
                richText = richText.Remove(start - offset, end - start);
                richText = richText.Insert(start - offset, emojiData.text);
                offset += emojiData.richText.Length - emojiData.text.Length;
                    
                i += emojiData.text.Length - 1;
            }
            
            richText = Regex.Replace(richText, MentionsPanel.MENTION_REGEX, MentionsPanel.MENTION_ID_PATTERN);
            richText = AdvancedInputFieldUtils.GetParsedText(richText);

            return richText;
        }

        public override (int, int) GetParsedTextLength(int characterLimit)
        {
            var length = 0;
            var characterLimitCaretPosition = 0;
            var text = InputField.Text;
            for (var i = 0; i < text.Length; i++)
            {
                var @char = text[i];
                if (NativeKeyboardManager.RichTextBindingEngine.TryGetBindingFromCodePoint(@char, out var tagData))
                {
                    var tagTextLength = tagData.name.Length;
                    
                    length += tagTextLength;
                    characterLimitCaretPosition += length <= characterLimit ? 1 : 0;
                }
                else if (NativeKeyboardManager.EmojiEngine.TryFindNextEmojiInText(text, i, out var emojiData))
                {
                    var emojiTextLength = emojiData.text.Length;
                    
                    length += 1;
                    characterLimitCaretPosition += length <= characterLimit ? emojiTextLength : 0;
                    i += emojiTextLength - 1;
                }
                else
                {
                    length++;
                    characterLimitCaretPosition += length > characterLimit ? 0 : 1;
                }
            }

            return (length, characterLimitCaretPosition);
        }

        public override void SetTextWithoutNotify(string input)
        {
            InputField.SetRichText(input);
        }
    }
}