using System.Collections.Generic;
using Bridge.Services.UserProfile;
using UIManaging.Common;
using UIManaging.Common.Args.Views.LastLevelsPanelArgs;
using UIManaging.Pages.FollowersPage.UI;

namespace UIManaging.Pages.UserProfile.Ui.ProfileHelper
{
    internal sealed class LocalUserProfileHelper : BaseUserProfileHelper
    {
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void OnEnable()
        {
            FollowersManager.LocalUserFollowed.OnChanged += OnFollowersOrFollowedChanged;
            FollowersManager.LocalUserFollower.OnChanged += OnFollowersOrFollowedChanged;
        }

        protected override void OnDisable()
        {
            FollowersManager.LocalUserFollowed.OnChanged -= OnFollowersOrFollowedChanged;
            FollowersManager.LocalUserFollower.OnChanged -= OnFollowersOrFollowedChanged;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override bool IsCurrentUser => true;
        protected override long UserGroupId => Bridge.Profile.GroupId;

        public override UserTaggedVideoListLoader InitTaggedLevelPanelArgs()
        {
            TaggedLevelPanelArgs = new UserTaggedVideoListLoader(VideoManager, PageManager, Bridge, UserGroupId);
            return TaggedLevelPanelArgs;
        }

        public override BaseFollowersPageArgs GetFollowersPageArgs(int tabIndex)
        {
            return new UserFollowersPageArgs(FollowersManager, tabIndex) {IsLocalUser = true};
        }

        public override UserProfileTaskVideosGridLoader InitTaskLevelPanelArgs()
        {
            return new UserProfileTaskVideosGridLoader(VideoManager, PageManager, Bridge, UserGroupId);
        }

        public override void UpdateRelatedUI(bool show)
        {
            base.UpdateRelatedUI(show);
            if (show) _horizontalLayout.childAlignment = UnityEngine.TextAnchor.MiddleLeft;
        }
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override BaseVideoListLoader CreateLevelsPanelArgs()
        {
            return new LocalUserVideoListLoader(PublishVideoHelper, VideoManager, PageManager, Bridge, LevelService);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnFollowersOrFollowedChanged(List<Profile> profiles)
        {
            RefreshKPIView();
        }
    }
}