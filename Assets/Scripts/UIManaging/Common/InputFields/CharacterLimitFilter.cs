using System.Text;
using AdvancedInputFieldPlugin;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;

namespace UIManaging.Common.InputFields
{
    public class CharacterLimitFilter: LiveProcessingFilter
    {
		[SerializeField] private int _characterLimit = 150;

        public int CharacterLimit => _characterLimit; 
        private StringBuilder StringBuilder => _stringBuilder ?? (_stringBuilder = new StringBuilder());
        
        [Inject] private SnackBarHelper _snackBarHelper;

		private StringBuilder _stringBuilder;

        //---------------------------------------------------------------------
        // Public 
        //---------------------------------------------------------------------

        public override TextEditFrame ProcessTextEditUpdate(TextEditFrame textEditFrame, TextEditFrame lastTextEditFrame)
		{
			if(textEditFrame.text == lastTextEditFrame.text) //No text change
			{
				return textEditFrame; //No processing needed, so allow change by returning current frame
			}
			else //Text change
			{
				if(textEditFrame.selectionStartPosition == textEditFrame.selectionEndPosition && lastTextEditFrame.selectionStartPosition != lastTextEditFrame.selectionEndPosition) //Selection cleared
				{
					int previousSelectionAmount = lastTextEditFrame.selectionEndPosition - lastTextEditFrame.selectionStartPosition;
					int insertAmount = textEditFrame.text.Length - (lastTextEditFrame.text.Length - previousSelectionAmount);
					if(insertAmount > 0) //Clear & insert
					{
						return ApplyCharacterLimit(textEditFrame);
					}
					else //Only clear
					{
						return textEditFrame; //No processing needed, so allow change by returning current frame
					}
				}
				else //No selection change
				{
					if(textEditFrame.selectionStartPosition > lastTextEditFrame.selectionStartPosition) //Text insert
					{
						return ApplyCharacterLimit(textEditFrame);
					}
					else if(textEditFrame.selectionStartPosition < lastTextEditFrame.selectionStartPosition) //Backwards delete
					{
						return textEditFrame; //No processing needed, so allow change by returning current frame
					}
					else //Forward delete
					{
						return textEditFrame; //No processing needed, so allow change by returning current frame
					}
				}
			}
		}
        
		/// <summary>Calculates the total character count, counting each emoji as a single character</summary>
        public int GetCharacterCount(string text)
		{
			var characterCount = 0;
			StringBuilder.Clear();

			var length = text.Length;
			for(var i = 0; i < length; i++)
			{
				var @char = text[i];
                if (NativeKeyboardManager.RichTextBindingEngine.TryGetBindingFromCodePoint(@char, out var tagData))
                {
					if(StringBuilder.Length > 0)
					{
						characterCount += StringBuilder.Length;
						StringBuilder.Clear();
					}

                    characterCount += tagData.name.Length;
                    i++;
                }

                if(NativeKeyboardManager.EmojiEngine.TryFindNextEmojiInText(text, i, out EmojiData emojiData))
				{
					if(StringBuilder.Length > 0)
					{
						characterCount += StringBuilder.Length;
						StringBuilder.Clear();
					}

					characterCount++; //Count emoji as 1 character
					i += (emojiData.text.Length - 1);
				}
				else
				{
					StringBuilder.Append(@char);
				}
			}

			if(StringBuilder.Length > 0)
			{
				characterCount += StringBuilder.Length;
				StringBuilder.Clear();
			}

			return characterCount;
		}
        
        //---------------------------------------------------------------------
        // Helpers 
        //---------------------------------------------------------------------

        private TextEditFrame ApplyCharacterLimit(TextEditFrame textEditFrame)
		{
			string text = textEditFrame.text;
			int position = textEditFrame.selectionStartPosition;
            var length = GetCharacterCount(text);

            if (length > _characterLimit)
            { 
                while (length > _characterLimit) //Need to recalculate character count each time here, because some emojis can consist out of multiple emojis which can exist independently
                {
                    RemovePreviousCharacter(ref text, ref position);
                    textEditFrame.text = text;
                    textEditFrame.selectionStartPosition = position;
                    textEditFrame.selectionEndPosition = position;

                    length = GetCharacterCount(text);
                }
                
                _snackBarHelper.ShowInformationSnackBar("Reached text limitation", 2);
            }

			return textEditFrame;
		}

        private void RemovePreviousCharacter(ref string text, ref int position)
		{
			if(NativeKeyboardManager.EmojiEngine.TryFindPreviousEmojiInText(text, position - 1, out EmojiData emojiData))
			{
				var emojiLength = emojiData.text.Length;
				position -= emojiLength;
				text = text.Remove(position, emojiLength);
			}
			else
			{
				position--;
				text = text.Remove(position, 1);
			}
		}
	}
}