using AdvancedInputFieldPlugin;
using UnityEngine;

namespace UIManaging.Pages.SharingPage.Ui
{
    public class DescriptionFilter: LiveProcessingFilter
    {
        [SerializeField] private FilterBasedMentionsHandler _mentionsHandler;
        [SerializeField] private FilterBasedHashtagsHandler _hashtagsHandler;
        
        public override TextEditFrame ProcessTextEditUpdate(TextEditFrame textEditFrame, TextEditFrame lastTextEditFrame)
        {
            if (_mentionsHandler.TryFindOccurrence(textEditFrame, out var mentionOccurence))
            {
                if (!_hashtagsHandler.IsActive && _mentionsHandler.ValidateConstraints(ref textEditFrame))
                {
                    _mentionsHandler.Process(mentionOccurence);
                }
                else
                {
                    return lastTextEditFrame;
                }
            }

            if (_hashtagsHandler.TryFindOccurrence(textEditFrame, out var hashtagOccurence))
            {
                if (_hashtagsHandler.ValidateConstraints(ref textEditFrame))
                {
                    _hashtagsHandler.Process(hashtagOccurence);
                }
                else
                {
                    return lastTextEditFrame;
                }
            }
            else
            {
                _hashtagsHandler.Hide();
            }
            
            return textEditFrame;
        }

        public override void OnRichTextEditUpdate(TextEditFrame richTextEditFrame, TextEditFrame lastRichTextEditFrame)
        {
            if (richTextEditFrame.selectionStartPosition != lastRichTextEditFrame.selectionStartPosition
             || richTextEditFrame.selectionEndPosition !=
                lastRichTextEditFrame.selectionEndPosition) //Caret or selection changed
            {
                _mentionsHandler.Hide();
                _hashtagsHandler.Hide();
            }
        }
    }
}