using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    [CreateAssetMenu(menuName = "L10N/ProfileLocalization", fileName = "ProfileLocalization")]
    public class ProfileLocalization : ScriptableObject
    {
        [SerializeField] private LocalizedString _draftsTextFormat;
        [SerializeField] private LocalizedString _draftsTextPluralFormat;
        [SerializeField] private LocalizedString _blockUserButton;
        [SerializeField] private LocalizedString _userBlockedSnackbarMessage;
        
        [SerializeField] private LocalizedString _profilePhotoUpdateSuccessSnackbarMessage;
        [SerializeField] private LocalizedString _profilePhotoUpdateFailedSnackbarMessage;
        [SerializeField] private LocalizedString _unableToChatWithNotFriendSnackbarMessage;
        
        [SerializeField] private LocalizedString _followRecommendationReasonPopularCreator;
        [SerializeField] private LocalizedString _followRecommendationReasonFollowedBy;
        [SerializeField] private LocalizedString _followRecommendationReasonFollowedByMultiple;
        [SerializeField] private LocalizedString _followRecommendationReasonSimilarInterests;
        [SerializeField] private LocalizedString _followRecommendationReasonFollowingYou;
        
        [SerializeField] private LocalizedString _remoteUserFriendListPlaceholder;
        [SerializeField] private LocalizedString _remoteUserFollowersListPlaceholder;
        [SerializeField] private LocalizedString _remoteUserFollowingListPlaceholder;

        [SerializeField] private LocalizedString _friendsTab;
        [SerializeField] private LocalizedString _followersTab;
        [SerializeField] private LocalizedString _followingTab;

        [SerializeField] private LocalizedString _invitedUserSignedUpTitleFormat;
        
        [SerializeField] private LocalizedString _messagesLockedSnackBar;
        
        public string DraftsTextFormat => _draftsTextFormat;
        public string DraftsTextPluralFormat => _draftsTextPluralFormat;
        public string BlockUserButton => _blockUserButton;
        public string UserBlockedSnackbarMessage => _userBlockedSnackbarMessage;
        public string ProfilePhotoUpdateSuccessSnackbarMessage => _profilePhotoUpdateSuccessSnackbarMessage;
        public string ProfilePhotoUpdateFailedSnackbarMessage => _profilePhotoUpdateFailedSnackbarMessage;
        public string UnableToChatWithNotFriendSnackbarMessage => _unableToChatWithNotFriendSnackbarMessage;
        public string FollowRecommendationReasonPopularCreator => _followRecommendationReasonPopularCreator;
        public string FollowRecommendationReasonFollowedBy => _followRecommendationReasonFollowedBy;
        public string FollowRecommendationReasonFollowedByMultiple => _followRecommendationReasonFollowedByMultiple;
        public string FollowRecommendationReasonSimilarInterests => _followRecommendationReasonSimilarInterests;
        public string FollowRecommendationReasonFollowingYou => _followRecommendationReasonFollowingYou;
        public string RemoteUserFriendListPlaceholder => _remoteUserFriendListPlaceholder;
        public string RemoteUserFollowersListPlaceholder => _remoteUserFollowersListPlaceholder;
        public string RemoteUserFollowingListPlaceholder => _remoteUserFollowingListPlaceholder;
        public string FriendsTab => _friendsTab;
        public string FollowersTab => _followersTab;
        public string FollowingTab => _followingTab;
        public string InvitedUserSignedUpTitleFormat => _invitedUserSignedUpTitleFormat;
        public string MessagesLockedSnackBar => _messagesLockedSnackBar;
    }
}