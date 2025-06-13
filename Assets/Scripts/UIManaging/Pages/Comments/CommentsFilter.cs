using AdvancedInputFieldPlugin;
using UIManaging.Common.InputFields;
using UIManaging.Pages.SharingPage.Ui;
using UnityEngine;

namespace UIManaging.Pages.Comments
{
    public class CommentsFilter: LiveProcessingFilter
    {
        [SerializeField] private FilterBasedMentionsHandler _mentionsHandler;
        [SerializeField] private CharacterLimitFilter _characterLimitFilter;
        
        public override TextEditFrame ProcessTextEditUpdate(TextEditFrame textEditFrame, TextEditFrame lastTextEditFrame)
        {
            // TODO: add support for list of filters
            textEditFrame = _characterLimitFilter.ProcessTextEditUpdate(textEditFrame, lastTextEditFrame);
            
            if (_mentionsHandler.TryFindOccurrence(textEditFrame, out var mentionOccurence))
            {
                if (_mentionsHandler.ValidateConstraints(ref textEditFrame))
                {
                    _mentionsHandler.Process(mentionOccurence);
                }
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
            }
        }
    }
}