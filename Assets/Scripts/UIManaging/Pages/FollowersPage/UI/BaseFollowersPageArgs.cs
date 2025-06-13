using System.Collections.Generic;
using System.Linq;
using Bridge.Services.UserProfile;
using Navigation.Core;
using UIManaging.Pages.Common.FollowersManagement;

namespace UIManaging.Pages.FollowersPage.UI
{
    public abstract class BaseFollowersPageArgs : PageArgs
    {
        public override PageId TargetPage => PageId.FollowersPage;

        public int InitialTabIndex { get; set; }
        public bool IsLocalUser { get; set; }

        public const int FRIENDS_TAB_INDEX = 0;
        public const int FOLLOWERS_TAB_INDEX = 1;
        public const int FOLLOWING_TAB_INDEX = 2;

        protected readonly FollowersManager FollowersManager;

        protected BaseFollowersPageArgs(FollowersManager followersManager)
        {
            FollowersManager = followersManager;
        }

        protected BaseFollowersPageArgs(FollowersManager followersManager, int initialTabIndex)
        {
            FollowersManager = followersManager;
            InitialTabIndex = initialTabIndex;
        }

        protected virtual void UnsubscribeFromEvents() { }
        protected virtual void SubscribeToEvents() { }

        protected void SetFollowers(IEnumerable<Profile> followers)
        {
            followers.ToList();
        }

        protected void SetFollowed(IEnumerable<Profile> followers)
        {
            followers.ToList();
        }
    }
}