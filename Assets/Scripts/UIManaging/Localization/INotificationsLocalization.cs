namespace UIManaging.Localization
{
    public interface INotificationsLocalization
    {
        string NotificationPageHeader { get; }
        string CommentOnYourVideoFormat { get; }
        string CommentOnVideoYouHaveCommentedFormat { get; }
        string CrewFriendJoinedFormat { get; }
        string CrewInvitationFormat { get; }
        string CrewJoinRequestedFormat { get; }
        string CrewJoinApprovedFormat { get; }
        string FriendNewVideoFormat { get; }
        string InvitationAcceptedFormat { get; }
        string NewFollowerFormat { get; }
        string NewLikeOnVideoFormat { get; }
        string NewMentionInCommentOnVideoFormat { get; }
        string NewMentionInReplyOnVideoFormat { get; }
        string NewMentionOnVideoFormat { get; }
        string NonCharacterTagOnVideoFormat { get; }
        string YourVideoRemixedFormat { get; }
        string YourVideoGotRatingFormat { get; }
        string SeasonLikesFormat { get; }
        string SeasonZeroLikesFormat { get; }
        string StyleBattleResultFormat { get; }
        string TaggedOnVideoFormat { get; }
        string VideoDeletedFormat { get; }
        string UserDoesNotExistSnackbarMessage { get; }
        string NoCrewCoordinatorRightsSnackbarMessage { get; }
        string NotACrewMemberSnackbarMessage { get; }
        string CrewRequestRespondButton { get; }
        string CrewRequestAcceptedButton { get; }
        string CrewRequestDeniedButton { get; }
        string ClaimUserInviteRewardButton { get; }
        string UserInviteRewardClaimedButton { get; }
        string TimePeriodOlder { get; }
        string TimePeriodThisMonth { get; }
        string TimePeriodThisWeek { get; }
        string TimePeriodToday { get; }
        string ShowAll { get; }
        string HideAll { get; }
    }
}