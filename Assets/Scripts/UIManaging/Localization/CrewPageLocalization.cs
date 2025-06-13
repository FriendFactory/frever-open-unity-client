using System;
using I2.Loc;
using UnityEngine;
using UnityEngine.Serialization;

namespace UIManaging.Localization
{
    [CreateAssetMenu(menuName = "L10N/CrewPageLocalization", fileName = "CrewPageLocalization")]
    public class CrewPageLocalization : ScriptableObject
    {
        [SerializeField] private LocalizedString[] _crewRanks;
            
        [SerializeField] private LocalizedString _tabChat;
        [SerializeField] private LocalizedString _tabTrophyHunt;
        [SerializeField] private LocalizedString _tabAbout;
        [SerializeField] private LocalizedString _searchHeader;
        
        [SerializeField] private LocalizedString _crewListItemFriendsCounterFormat;
        [SerializeField] private LocalizedString _crewListItemFollowersCounterFormat;
        [SerializeField] private LocalizedString _crewListItemFollowingCounterFormat;
        
        [SerializeField] private LocalizedString _crewListItemFriendsPluralCounterFormat;
        [SerializeField] private LocalizedString _crewListItemFollowersPluralCounterFormat;
        [SerializeField] private LocalizedString _crewListItemFollowingPluralCounterFormat;
        
        [SerializeField] private LocalizedString _editAccessRestrictedSnackbarMessage;
        [SerializeField] private LocalizedString _inviteSuccessSnackbarMessage;
        [SerializeField] private LocalizedString _inviteFailedSnackbarMessage;
        
        [SerializeField] private LocalizedString _crewNameUpdatedSnackbarMessage;
        [SerializeField] private LocalizedString _crewDescriptionUpdatedSnackbarMessage;
        [SerializeField] private LocalizedString _crewPhotoUpdatedSnackbarMessage;
        
        [SerializeField] private LocalizedString _blockedUsersTitle;
        
        [SerializeField] private LocalizedString _userItemInviteButton;
        [SerializeField] private LocalizedString _userItemInviteSentButton;
        
        [SerializeField] private LocalizedString _joinCrewButton;
        [SerializeField] private LocalizedString _joinRequestSentButton;
        [SerializeField] private LocalizedString _sendJoinRequestButton;
        [SerializeField] private LocalizedString _acceptInvitationButton;
        [SerializeField] private LocalizedString _alreadyInAnotherCrewJoinButton;
        [SerializeField] private LocalizedString _joinStateLoadingButton;
        [SerializeField] private LocalizedString _joinCrewIsFullButton;
        
        [SerializeField] private LocalizedString _transferOwnershipSuccessSnackbarMessageFormat;
        [SerializeField] private LocalizedString _motdPostedSnackbarMessage;
        [SerializeField] private LocalizedString _leftCrewSnackbarMessage;
        
        [SerializeField] private LocalizedString _notAMemberSnackbarMessage;
        [SerializeField] private LocalizedString _crewIsFullSnackbarMessage;
        [SerializeField] private LocalizedString _memberNotFoundSnackbarMessage;
        [SerializeField] private LocalizedString _userRequestAcceptedSnackbarMessageFormat;
        [SerializeField] private LocalizedString _userRequestRejectedSnackbarMessageFormat;
        [SerializeField] private LocalizedString _userRemovedSnackbarMessageFormat;
        [SerializeField] private LocalizedString _userIsInAnotherCrewSnackbarMessageFormat;
        [SerializeField] private LocalizedString _crewNameIsAlreadyTakenSnackbarMessage;
        [SerializeField] private LocalizedString _roleUpdatedSnackbarMessage;
        [SerializeField] private LocalizedString _inviteAccessRestrictedSnackbarMessage;
        [SerializeField] private LocalizedString _joinRequestSentSnackbarMessage;
        
        [SerializeField] private LocalizedString _kickUserPopupTitle;
        [SerializeField] private LocalizedString _kickUserPopupDescriptionFormat;
        [SerializeField] private LocalizedString _kickUserPopupConfirmButton;
        [SerializeField] private LocalizedString _kickUserPopupCancelButton;
        
        [SerializeField] private LocalizedString _demoteYourselfPopupTitle;
        [SerializeField] private LocalizedString _demoteYourselfPopupDescription;
        [SerializeField] private LocalizedString _demoteYourselfPopupConfirmButton;
        [SerializeField] private LocalizedString _demoteYourselfPopupCancelButton;
        
        [SerializeField] private LocalizedString _deleteCrewPopupTitle;
        [SerializeField] private LocalizedString _deleteCrewPopupDescription;
        [SerializeField] private LocalizedString _deleteCrewPopupConfirmButton;
        [SerializeField] private LocalizedString _deleteCrewPopupCancelButton;
        
        [SerializeField] private LocalizedString _deleteCrewIsFullPopupTitle;
        [SerializeField] private LocalizedString _deleteCrewIsFullPopupDescription;
        [SerializeField] private LocalizedString _deleteCrewIsFullPopupConfirmButton;
        
        [SerializeField] private LocalizedString _transferOwnershipPopupTitle;
        [SerializeField] private LocalizedString _transferOwnershipPopupDescription;
        [SerializeField] private LocalizedString _transferOwnershipPopupConfirmButton;
        [SerializeField] private LocalizedString _transferOwnershipPopupCancelButton;
        [SerializeField] private LocalizedString _transferOwnershipFirstSnackbarMessage;
        
        [SerializeField] private LocalizedString _updatePhotoPopupTitle;
        [SerializeField] private LocalizedString _updatePhotoPopupDescription;
        [SerializeField] private LocalizedString _updatePhotoPopupConfirmButton;
        [SerializeField] private LocalizedString _updatePhotoPopupCancelButton;
        
        [SerializeField] private LocalizedString _leavePopupTitle;
        [SerializeField] private LocalizedString _leavePopupDescription;
        [SerializeField] private LocalizedString _leavePopupConfirmButton;
        [SerializeField] private LocalizedString _leavePopupCancelButton;
        
        [SerializeField] private LocalizedString _eraseSettingChangesPopupTitle;
        [SerializeField] private LocalizedString _eraseSettingChangesPopupDescription;
        [SerializeField] private LocalizedString _eraseSettingChangesPopupConfirmButton;
        [SerializeField] private LocalizedString _eraseSettingChangesPopupCancelButton;
        
        [SerializeField] private LocalizedString _trophyHuntWeekFormat;
        [SerializeField] private LocalizedString _trophyHuntTimeLeftFormat;
        [SerializeField] private LocalizedString _trophyHuntRewardClaimedSnackbarMessage;

        [SerializeField] private LocalizedString _lastSeenOnlineTimeFormat;
        [SerializeField] private LocalizedString _lastUpdatedTimeFormat;
        
        [SerializeField] private LocalizedString _topThisWeekOption;
        [SerializeField] private LocalizedString _topLastWeekOption;

        [SerializeField] private LocalizedString _onlineNow;
        
        public string GetRankLocalized(long rankId) => _crewRanks[rankId - 1];

        public string TabChat => _tabChat;
        public string TabTrophyHunt => _tabTrophyHunt;
        public string TabAbout => _tabAbout;
        public string SearchHeader => _searchHeader;
        
        public string CrewListItemFriendsCounterFormat => _crewListItemFriendsCounterFormat;
        public string CrewListItemFollowersCounterFormat => _crewListItemFollowersCounterFormat;
        public string CrewListItemFollowingCounterFormat => _crewListItemFollowingCounterFormat;
        
        public string CrewListItemFriendsPluralCounterFormat => _crewListItemFriendsPluralCounterFormat;
        public string CrewListItemFollowersPluralCounterFormat => _crewListItemFollowersPluralCounterFormat;
        public string CrewListItemFollowingPluralCounterFormat => _crewListItemFollowingPluralCounterFormat;
        
        public string EditAccessRestrictedSnackbarMessage => _editAccessRestrictedSnackbarMessage;
        public string InviteSuccessSnackbarMessage => _inviteSuccessSnackbarMessage;
        public string InviteFailedSnackbarMessage => _inviteFailedSnackbarMessage;
        public string BlockedUsersTitle => _blockedUsersTitle;
        public string UserItemInviteButton => _userItemInviteButton;
        public string UserItemInviteSentButton => _userItemInviteSentButton;
        
        public string CrewNameUpdatedSnackbarMessage => _crewNameUpdatedSnackbarMessage;
        public string CrewDescriptionUpdatedSnackbarMessage => _crewDescriptionUpdatedSnackbarMessage;
        public string CrewPhotoUpdatedSnackbarMessage => _crewPhotoUpdatedSnackbarMessage;
        
        public string JoinCrewButton => _joinCrewButton;
        public string JoinRequestSentButton => _joinRequestSentButton;
        public string SendJoinRequestButton => _sendJoinRequestButton;
        public string AcceptInvitationButton => _acceptInvitationButton;
        public string AlreadyInAnotherCrewJoinButton => _alreadyInAnotherCrewJoinButton;
        public string JoinStateLoadingButton => _joinStateLoadingButton;
        public string JoinCrewIsFullButton => _joinCrewIsFullButton;
        
        public string TransferOwnershipSuccessSnackbarMessageFormat => _transferOwnershipSuccessSnackbarMessageFormat;
        public string MotdPostedSnackbarMessage => _motdPostedSnackbarMessage;
        public string LeftCrewSnackbarMessage => _leftCrewSnackbarMessage;

        public string NotAMemberSnackbarMessage => _notAMemberSnackbarMessage;
        public string CrewIsFullSnackbarMessage => _crewIsFullSnackbarMessage;
        public string MemberNotFoundSnackbarMessage => _memberNotFoundSnackbarMessage;
        public string UserRequestAcceptedSnackbarMessageFormat => _userRequestAcceptedSnackbarMessageFormat;
        public string UserRequestRejectedSnackbarMessageFormat => _userRequestRejectedSnackbarMessageFormat;

        public string UserRemovedSnackbarMessageFormat => _userRemovedSnackbarMessageFormat;
        public string UserIsInAnotherCrewSnackbarMessageFormat => _userIsInAnotherCrewSnackbarMessageFormat;
        public string CrewNameIsAlreadyTakenSnackbarMessage => _crewNameIsAlreadyTakenSnackbarMessage;
        public string RoleUpdatedSnackbarMessage => _roleUpdatedSnackbarMessage;
        public string InviteAccessRestrictedSnackbarMessage => _inviteAccessRestrictedSnackbarMessage;
        public string JoinRequestSentSnackbarMessage => _joinRequestSentSnackbarMessage;
        
        public string KickUserPopupTitle => _kickUserPopupTitle;
        public string KickUserPopupDescription => _kickUserPopupDescriptionFormat;
        public string KickUserPopupConfirmButton => _kickUserPopupConfirmButton;
        public string KickUserPopupCancelButton => _kickUserPopupCancelButton;
        
        public string DemoteYourselfPopupTitle => _demoteYourselfPopupTitle;
        public string DemoteYourselfPopupDescription => _demoteYourselfPopupDescription;
        public string DemoteYourselfPopupConfirmButton => _demoteYourselfPopupConfirmButton;
        public string DemoteYourselfPopupCancelButton => _demoteYourselfPopupCancelButton;
        
        public string DeleteCrewPopupTitle => _deleteCrewPopupTitle;
        public string DeleteCrewPopupDescription => _deleteCrewPopupDescription;
        public string DeleteCrewPopupConfirmButton => _deleteCrewPopupConfirmButton;
        public string DeleteCrewPopupCancelButton => _deleteCrewPopupCancelButton;
        
        public string DeleteCrewIsFullPopupTitle => _deleteCrewIsFullPopupTitle;
        public string DeleteCrewIsFullPopupDescription => _deleteCrewIsFullPopupDescription;
        public string DeleteCrewIsFullPopupConfirmButton => _deleteCrewIsFullPopupConfirmButton;
        
        public string TransferOwnershipPopupTitle => _transferOwnershipPopupTitle;
        public string TransferOwnershipPopupDescription => _transferOwnershipPopupDescription;
        public string TransferOwnershipPopupConfirmButton => _transferOwnershipPopupConfirmButton;
        public string TransferOwnershipPopupCancelButton => _transferOwnershipPopupCancelButton;
        public string TransferOwnershipFirstSnackbarMessage => _transferOwnershipFirstSnackbarMessage;
        
        public string UpdatePhotoPopupTitle => _updatePhotoPopupTitle;
        public string UpdatePhotoPopupDescription => _updatePhotoPopupDescription;
        public string UpdatePhotoPopupConfirmButton => _updatePhotoPopupConfirmButton;
        public string UpdatePhotoPopupCancelButton => _updatePhotoPopupCancelButton;
        
        public string LeavePopupTitle => _leavePopupTitle;
        public string LeavePopupDescription => _leavePopupDescription;
        public string LeavePopupConfirmButton => _leavePopupConfirmButton;
        public string LeavePopupCancelButton => _leavePopupCancelButton;
        
        public string EraseSettingChangesPopupTitle => _eraseSettingChangesPopupTitle;
        public string EraseSettingChangesPopupDescription => _eraseSettingChangesPopupDescription;
        public string EraseSettingChangesPopupConfirmButton => _eraseSettingChangesPopupConfirmButton;
        public string EraseSettingChangesPopupCancelButton => _eraseSettingChangesPopupCancelButton;
        
        public string TrophyHuntWeekFormat => _trophyHuntWeekFormat;
        public string TrophyHuntTimeLeftFormat => _trophyHuntTimeLeftFormat;
        public string TrophyHuntRewardClaimedSnackbarMessage => _trophyHuntRewardClaimedSnackbarMessage;
        public string LastSeenOnlineTimeFormat => _lastSeenOnlineTimeFormat;
        public string LastUpdatedTimeFormat => _lastUpdatedTimeFormat;
        
        public string TopThisWeekOption => _topThisWeekOption;
        public string TopLastWeekOption => _topLastWeekOption;
        public string OnlineNow => _onlineNow;
    }
}