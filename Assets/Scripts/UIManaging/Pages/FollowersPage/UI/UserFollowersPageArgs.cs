using System.Collections.Generic;
using Bridge.Services.UserProfile;
using UIManaging.Pages.Common.FollowersManagement;

namespace UIManaging.Pages.FollowersPage.UI
{
    public class UserFollowersPageArgs : BaseFollowersPageArgs
    {
        public UserFollowersPageArgs(FollowersManager followersManager, int initialTabIndex) : base(followersManager, initialTabIndex)
        {
            SubscribeToEvents();
        }

        ~UserFollowersPageArgs()
        {
            UnsubscribeFromEvents();
        }

        protected override void UnsubscribeFromEvents()
        {
            base.UnsubscribeFromEvents();
            
            if (FollowersManager != null)
            {
                FollowersManager.LocalUserFollowed.OnChanged -= OnFollowedChanged;
                FollowersManager.LocalUserFollower.OnChanged -= OnFollowersChanged;
            }
        }

        protected override void SubscribeToEvents()
        {
            base.SubscribeToEvents();
            
            FollowersManager.LocalUserFollowed.OnChanged += OnFollowedChanged;
            FollowersManager.LocalUserFollower.OnChanged += OnFollowersChanged;
        }

        private void OnFollowedChanged(List<Profile> followed)
        {
            SetFollowed(followed);
        }
        
        private void OnFollowersChanged(List<Profile> followers)
        {
            SetFollowers(followers);
        }
    }
}