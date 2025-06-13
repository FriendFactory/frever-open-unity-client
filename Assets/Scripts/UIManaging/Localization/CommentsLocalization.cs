using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    [CreateAssetMenu(menuName = "L10N/CommentsLocalization", fileName = "CommentsLocalization")]
    public class CommentsLocalization : ScriptableObject
    {
        [SerializeField] private LocalizedString _defaultInputPlaceholder;
        [SerializeField] private LocalizedString _replyInputPlaceholderFormat;
        
        [SerializeField] private LocalizedString _commentsReplyOption;
        [SerializeField] private LocalizedString _commentsCopyOption;
        
        [SerializeField] private LocalizedString _commentAddedSnackbarMessage;
        [SerializeField] private LocalizedString _reachedTextLimitSnackbarMessage;
        [SerializeField] private LocalizedString _reachedMentionLimitSnackbarMessage;
        [SerializeField] private LocalizedString _commentCopiedSnackbarMessage;
        
        public string DefaultInputPlaceholder => _defaultInputPlaceholder;
        public string ReplyInputPlaceholderFormat => _replyInputPlaceholderFormat;
        
        public string CommentsReplyOption => _commentsReplyOption;
        public string CommentsCopyOption => _commentsCopyOption;
        
        public string CommentAddedSnackbarMessage => _commentAddedSnackbarMessage;
        public string ReachedTextLimitSnackbarMessage => _reachedTextLimitSnackbarMessage;
        public string ReachedMentionLimitSnackbarMessage => _reachedMentionLimitSnackbarMessage;
        public string CommentCopiedSnackbarMessage => _commentCopiedSnackbarMessage;
    }
}