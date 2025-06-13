using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    [CreateAssetMenu(menuName = "L10N/UserListItemLocalization", fileName = "UserListItemLocalization")]
    public class UserListItemLocalization : ScriptableObject
    {
        [SerializeField] private LocalizedString _followButton;
        [SerializeField] private LocalizedString _followBackButton;
        [SerializeField] private LocalizedString _followingButton;
        [SerializeField] private LocalizedString _friendsButton;
        [SerializeField] private LocalizedString _followersCounterTextFormat;
        [SerializeField] private LocalizedString _unfollowPopupUnfollowOption;
        
        [SerializeField] private LocalizedString _followUsernameButtonFormat;
        [SerializeField] private LocalizedString _followBackUsernameButtonFormat;
        
        public string FollowButton => _followButton;
        public string FollowBackButton => _followBackButton;
        public string FollowingButton => _followingButton;
        public string FriendsButton => _friendsButton;
        public string FollowersCounterTextFormat => _followersCounterTextFormat;
        public string UnfollowPopupUnfollowOption => _unfollowPopupUnfollowOption;
        
        public string FollowUsernameButtonFormat => _followUsernameButtonFormat;
        public string FollowBackUsernameButtonFormat => _followBackUsernameButtonFormat;
    }
}