using UIManaging.Common;
using UIManaging.Common.Args.Views.LastLevelsPanelArgs;
using UIManaging.Pages.FollowersPage.UI;

namespace UIManaging.Pages.UserProfile.Ui.ProfileHelper
{
    internal sealed class RemoteUserProfileHelper : BaseUserProfileHelper
    {
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void OnEnable()
        {         
            FollowersManager.OnRemoteUserFollowedChangedEvent += OnFollowerOrFollowedChanged;
            FollowersManager.OnRemoteUserFollowerChangedEvent += OnFollowerOrFollowedChanged;
        }

        protected override void OnDisable()
        {
            FollowersManager.OnRemoteUserFollowedChangedEvent -= OnFollowerOrFollowedChanged;
            FollowersManager.OnRemoteUserFollowerChangedEvent -= OnFollowerOrFollowedChanged;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override bool IsCurrentUser => false;
        protected override long UserGroupId => ProfilePage.OpenPageArgs.GroupId;

        public override UserTaggedVideoListLoader InitTaggedLevelPanelArgs()
        {
            var groupId = ProfilePage.OpenPageArgs.GroupId;
            TaggedLevelPanelArgs = new UserTaggedVideoListLoader(VideoManager, PageManager, Bridge, groupId);
            return TaggedLevelPanelArgs;
        }

        public override BaseFollowersPageArgs GetFollowersPageArgs(int tabIndex)
        {
            return new RemoteUserFollowersPageArgs(UserGroupId, FollowersManager, tabIndex);
        }

        public override UserProfileTaskVideosGridLoader InitTaskLevelPanelArgs()
        {
            return new UserProfileTaskVideosGridLoader(VideoManager, PageManager, Bridge, UserGroupId);
        }

        public override void UpdateRelatedUI(bool show)
        {
            base.UpdateRelatedUI(show);
            if (show) _horizontalLayout.childAlignment = UnityEngine.TextAnchor.MiddleCenter;
        }
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override BaseVideoListLoader CreateLevelsPanelArgs()
        {
            return new RemoteUserVideoListLoader(VideoManager, PageManager, Bridge, UserGroupId);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnFollowerOrFollowedChanged(long userGroupId)
        {
            if (userGroupId == UserGroupId)
            {
                RefreshKPIView();
            }
        }
    }
}