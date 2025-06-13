using System;
using Bridge.Services.UserProfile;

namespace UIManaging.Pages.FollowersPage.UI
{
    public class FollowerViewModel
    {
        public Action<long> OnFollow;
        public Action<long> OnUnfollow;
        public bool IsFollowing;
        public Profile UserProfile { get; private set; }
        public long GroupId { get; }
        public bool IsContact { get; }

        public FollowerViewModel(Profile userProfile, bool isContact = false)
        {
            UserProfile = userProfile;
            IsContact = isContact;
            GroupId = userProfile.MainGroupId;
        }

        public void SetProfile(Profile profile)
        {
            UserProfile = profile;
        }
    }
}
