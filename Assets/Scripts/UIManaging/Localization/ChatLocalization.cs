using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    [CreateAssetMenu(menuName = "L10N/ChatLocalization", fileName = "ChatLocalization")]
    public class ChatLocalization : ScriptableObject
    {
        [SerializeField] private LocalizedString _likeButton;
        [SerializeField] private LocalizedString _unlikeButton;
        [SerializeField] private LocalizedString _replyButton;
        
        [SerializeField] private LocalizedString _blockUserPopupTitle;
        [SerializeField] private LocalizedString _blockUserPopupDescription;
        [SerializeField] private LocalizedString _blockUserPopupConfirmButton;
        [SerializeField] private LocalizedString _blockUserPopupCancelButton;
        
        [SerializeField] private LocalizedString _reportPopupTitle;
        [SerializeField] private LocalizedString _reportPopupDescription;
        [SerializeField] private LocalizedString _reportPopupConfirmButton;
        [SerializeField] private LocalizedString _reportPopupCancelButton;
        
        [SerializeField] private LocalizedString _reportSuccessPopupTitle;
        [SerializeField] private LocalizedString _reportSuccessPopupDescription;
        [SerializeField] private LocalizedString _reportSuccessPopupConfirmButton;
        [SerializeField] private LocalizedString _cantReportEmptyChatSnackbarMessage;
        
        [SerializeField] private LocalizedString _leaveGroupPopupTitle;
        [SerializeField] private LocalizedString _leaveGroupPopupDescription;
        [SerializeField] private LocalizedString _leaveGroupPopupConfirmButton;
        [SerializeField] private LocalizedString _leaveGroupPopupCancelButton;
        
        [SerializeField] private LocalizedString _introPanelTitle;
        [SerializeField] private LocalizedString _replyToTitleFormat;
        
        [SerializeField] private LocalizedString _deleteMessageButton;
        [SerializeField] private LocalizedString _reportMessageButton;
        
        [SerializeField] private LocalizedString _renameSuccessSnackbarMessage;
        
        public string LikeButton => _likeButton;
        public string UnlikeButton => _unlikeButton;
        
        public string BlockUserPopupTitle => _blockUserPopupTitle;
        public string BlockUserPopupDescription => _blockUserPopupDescription;
        public string BlockUserPopupConfirmButton => _blockUserPopupConfirmButton;
        public string BlockUserPopupCancelButton => _blockUserPopupCancelButton;
        
        public string ReportPopupTitle => _reportPopupTitle;
        public string ReportPopupDescription => _reportPopupDescription;
        public string ReportPopupConfirmButton => _reportPopupConfirmButton;
        public string ReportPopupCancelButton => _reportPopupCancelButton;
        
        public string ReportSuccessPopupTitle => _reportSuccessPopupTitle;
        public string ReportSuccessPopupDescription => _reportSuccessPopupDescription;
        public string ReportSuccessPopupConfirmButton => _reportSuccessPopupConfirmButton;
        
        public string CantReportEmptyChatSnackbarMessage => _cantReportEmptyChatSnackbarMessage;
        
        public string LeaveGroupPopupTitle => _leaveGroupPopupTitle;
        public string LeaveGroupPopupDescription => _leaveGroupPopupDescription;
        public string LeaveGroupPopupConfirmButton => _leaveGroupPopupConfirmButton;
        public string LeaveGroupPopupCancelButton => _leaveGroupPopupCancelButton;
        
        public string IntroPanelTitle => _introPanelTitle;
        public string ReplyToTitleFormat => _replyToTitleFormat;
        
        public string DeleteMessageButton => _deleteMessageButton;
        public string ReportMessageButton => _reportMessageButton;

        public string RenameSuccessSnackbarMessage => _renameSuccessSnackbarMessage;
    }
}