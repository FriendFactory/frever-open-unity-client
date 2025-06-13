using Bridge.Services.UserProfile;

namespace UIManaging.Common.Args.Buttons
{
    public class FollowUserButtonArgs
    {
        public long UserGroupId { get; }
        public bool IsFollowedBy { get; }
        public bool IsFollowing { get; set;}
        public bool IsFriends => IsFollowing && IsFollowedBy;
        public Profile Profile { get; }

        public FollowUserButtonArgs(Profile profile)
        {
            Profile = profile;
            UserGroupId = profile.MainGroupId;
            IsFollowedBy = profile.UserFollowsYou;
            IsFollowing = profile.YouFollowUser;
        }
    }
}

