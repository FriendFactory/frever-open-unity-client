using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.FollowersPage.UI
{
    public class FollowMultipleFollowerView : FollowerView
    {
        
        [SerializeField] private GameObject _followButtonParent;
        [SerializeField] private GameObject _unfollowButtonParent;
        [SerializeField] private Button _followButton;
        [SerializeField] private Button _unfollowButton;

        protected override void OnInitialized()
        {
            AllowGoToUserProfile = false;
            base.OnInitialized();
            _followButton.onClick.AddListener(FollowUser);
            _unfollowButton.onClick.AddListener(UnFollowUser);
            
            if (ContextData.IsFollowing)
            {
                FollowUser();
            }
            else
            {
                UnFollowUser();
            }
        }
        
        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            _followButton.onClick.RemoveListener(FollowUser);
            _unfollowButton.onClick.RemoveListener(UnFollowUser);
        }

        private void FollowUser()
        {
            ContextData.IsFollowing = true;
            _unfollowButtonParent.SetActive(true);
            _followButtonParent.SetActive(false);
            ContextData.OnFollow?.Invoke(ContextData.UserProfile.MainGroupId);
        }
        
        private void UnFollowUser()
        {
            ContextData.IsFollowing = false;
            _unfollowButtonParent.SetActive(false);
            _followButtonParent.SetActive(true);
            
            ContextData.OnUnfollow?.Invoke(ContextData.UserProfile.MainGroupId);
                
        }

    }
}
