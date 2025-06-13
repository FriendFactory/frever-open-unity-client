using System;
using Bridge.Services.UserProfile;

namespace UIManaging.Common.Args.Buttons
{
    public class MultipleFollowUserButtonArgs: FollowUserButtonArgs
    {
        public Action<long> OnFollow { get; }
        public Action<long> OnUnfollow { get; }

        public MultipleFollowUserButtonArgs(Profile profile, Action<long> onFollow = null, Action<long> onUnfollow = null): base(profile)
        {
            OnFollow = onFollow;
            OnUnfollow = onUnfollow;
        }
    }
}

