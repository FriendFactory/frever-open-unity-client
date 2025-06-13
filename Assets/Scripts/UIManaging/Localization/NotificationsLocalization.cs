using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    [CreateAssetMenu(menuName = "L10N/NotificationsLocalization", fileName = "NotificationsLocalization")]
    public class NotificationsLocalization : ScriptableObject, INotificationsLocalization
    {
        [SerializeField] private LocalizedString _notificationPageHeader;
        
        [SerializeField] private LocalizedString _commentOnYourVideoFormat;
        [SerializeField] private LocalizedString _commentOnVideoYouHaveCommentedFormat;
        [SerializeField] private LocalizedString _crewFriendJoinedFormat;
        [SerializeField] private LocalizedString _crewInvitiationFormat;
        [SerializeField] private LocalizedString _crewRequestRespondButton;
        [SerializeField] private LocalizedString _crewJoinApprovedFormat;
        [SerializeField] private LocalizedString _crewJoinRequestedFormat;
        [SerializeField] private LocalizedString _friendNewVideoFormat;
        [SerializeField] private LocalizedString _invitationAcceptedFormat;
        [SerializeField] private LocalizedString _newFollowerFormat;
        [SerializeField] private LocalizedString _newLikeOnVideoFormat;
        [SerializeField] private LocalizedString _newLikesOnVideoFormat;
        [SerializeField] private LocalizedString _newMentionInCommentOnVideoFormat;
        [SerializeField] private LocalizedString _newMentionInReplyOnVideoFormat;
        [SerializeField] private LocalizedString _newMentionOnVideoFormat;
        [SerializeField] private LocalizedString _nonCharacterTagOnVideoFormat;
        [SerializeField] private LocalizedString _yourVideoRemixedFormat;
        [SerializeField] private LocalizedString _yourVideoGotRatingFormat;
        [SerializeField] private LocalizedString _seasonLikesFormat;
        [SerializeField] private LocalizedString _seasonZeroLikesFormat;
        [SerializeField] private LocalizedString _styleBattleResultFormat;
        [SerializeField] private LocalizedString _taggedOnVideoFormat;
        [SerializeField] private LocalizedString _videoDeletedFormat;
        
        [SerializeField] private LocalizedString _userDoesNotExistSnackbarMessage;
        [SerializeField] private LocalizedString _noCrewCoordinatorRightsSnackbarMessage;
        [SerializeField] private LocalizedString _notACrewMemberSnackbarMessage;
        [SerializeField] private LocalizedString _crewRequestAcceptedButton;
        [SerializeField] private LocalizedString _crewRequestDeniedButton;
        [SerializeField] private LocalizedString _claimUserInviteRewardButton;
        [SerializeField] private LocalizedString _userInviteRewardClaimedButton;
        
        [SerializeField] private LocalizedString _timePeriodOlder;
        [SerializeField] private LocalizedString _timePeriodThisMonth;
        [SerializeField] private LocalizedString _timePeriodThisWeek;
        [SerializeField] private LocalizedString _timePeriodThisToday;
        [SerializeField] private LocalizedString _showAll;
        [SerializeField] private LocalizedString _hideAll;
        
        public string NotificationPageHeader => _notificationPageHeader;
        
        public string CommentOnYourVideoFormat => _commentOnYourVideoFormat; 
        public string CommentOnVideoYouHaveCommentedFormat => _commentOnVideoYouHaveCommentedFormat;
        public string CrewFriendJoinedFormat => _crewFriendJoinedFormat;
        public string CrewInvitationFormat => _crewInvitiationFormat;
        public string CrewJoinRequestedFormat => _crewJoinRequestedFormat;
        public string CrewJoinApprovedFormat => _crewJoinApprovedFormat;
        public string FriendNewVideoFormat => _friendNewVideoFormat;
        public string InvitationAcceptedFormat => _invitationAcceptedFormat;
        public string NewFollowerFormat => _newFollowerFormat; 
        public string NewLikeOnVideoFormat => _newLikeOnVideoFormat;
        public string NewLikesOnVideoFormat => _newLikesOnVideoFormat;
        public string NewMentionInCommentOnVideoFormat => _newMentionInCommentOnVideoFormat;
        public string NewMentionInReplyOnVideoFormat  => _newMentionInReplyOnVideoFormat;
        public string NewMentionOnVideoFormat => _newMentionOnVideoFormat; 
        public string NonCharacterTagOnVideoFormat => _nonCharacterTagOnVideoFormat;
        public string YourVideoRemixedFormat => _yourVideoRemixedFormat;
        public string YourVideoGotRatingFormat => _yourVideoGotRatingFormat;
        public string SeasonLikesFormat => _seasonLikesFormat;
        public string SeasonZeroLikesFormat => _seasonZeroLikesFormat; 
        public string StyleBattleResultFormat => _styleBattleResultFormat;
        public string TaggedOnVideoFormat => _taggedOnVideoFormat;
        public string VideoDeletedFormat => _videoDeletedFormat; 
        
        public string UserDoesNotExistSnackbarMessage => _userDoesNotExistSnackbarMessage; 
        public string NoCrewCoordinatorRightsSnackbarMessage => _noCrewCoordinatorRightsSnackbarMessage;
        public string NotACrewMemberSnackbarMessage => _notACrewMemberSnackbarMessage;
        public string CrewRequestRespondButton => _crewRequestRespondButton;
        public string CrewRequestAcceptedButton => _crewRequestAcceptedButton;
        public string CrewRequestDeniedButton => _crewRequestDeniedButton; 
        public string ClaimUserInviteRewardButton => _claimUserInviteRewardButton;
        public string UserInviteRewardClaimedButton => _userInviteRewardClaimedButton;
        public string TimePeriodOlder => _timePeriodOlder;
        public string TimePeriodThisMonth => _timePeriodThisMonth;
        public string TimePeriodThisWeek => _timePeriodThisWeek;
        public string TimePeriodToday => _timePeriodThisToday;
        public string ShowAll => _showAll;
        public string HideAll => _hideAll;
    }
}