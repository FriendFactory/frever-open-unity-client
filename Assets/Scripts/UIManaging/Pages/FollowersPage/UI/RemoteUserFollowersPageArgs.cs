using System.Collections.Generic;
using Bridge.Services.UserProfile;
using UIManaging.Pages.Common.FollowersManagement;

namespace UIManaging.Pages.FollowersPage.UI
{
    public class RemoteUserFollowersPageArgs : BaseFollowersPageArgs
    {
        public long GroupId { get; }
        
        public RemoteUserFollowersPageArgs(long groupId, FollowersManager followersManager) : base(followersManager)
        {
            GroupId = groupId;
            SubscribeToEvents();
        }

        public RemoteUserFollowersPageArgs(long groupId, FollowersManager followersManager, int initialTabIndex) : base(followersManager, initialTabIndex)
        {
            GroupId = groupId;
            SubscribeToEvents();
        }

        ~RemoteUserFollowersPageArgs()
        {
            UnsubscribeFromEvents();
        }

        protected override void UnsubscribeFromEvents()
        {
            base.UnsubscribeFromEvents();
            
            if (FollowersManager != null)
            {
                FollowersManager.GetRemoteUserFollowed(GroupId).OnChanged -= OnFollowedChanged;
                FollowersManager.GetRemoteUserFollower(GroupId).OnChanged -= OnFollowersChanged;
            }
        }

        protected override void SubscribeToEvents()
        {
            base.SubscribeToEvents();
            FollowersManager.GetRemoteUserFollowed(GroupId).OnChanged += OnFollowedChanged;
            FollowersManager.GetRemoteUserFollower(GroupId).OnChanged += OnFollowersChanged;
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